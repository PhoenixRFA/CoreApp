using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MVCApp.Infrastructure;
using MVCApp.Infrastructure.AuthorizationRequirements;
using MVCApp.Infrastructure.Constraints;
using MVCApp.Infrastructure.Filters;
using MVCApp.Infrastructure.HealthCheck;
using MVCApp.Infrastructure.Middleware;
using MVCApp.Infrastructure.ModelBinders;
using MVCApp.Infrastructure.ValueProviders;
using MVCApp.Models;
using MVCApp.Services;
using MVCApp.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MVCApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TestappdbContext>(opts =>
            {
                opts.UseSqlServer(connection);
                opts.LogTo(x => Debug.WriteLine(x), LogLevel.Information);
            });


            //Custom aithorization policy requirement handler
            services.AddTransient<IAuthorizationHandler, AgeHandler>();
            //Dynamic policy provider
            services.AddSingleton<IAuthorizationPolicyProvider, MinimumAgePolicyProvider>();
            //Claims transformer
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            //Add authentification services
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opts=>
                {
                    opts.LoginPath = new PathString("/Account/Login");
                });
            services.AddAuthorization(opts =>
            {
                AuthorizationPolicy dafault = opts.DefaultPolicy;
                opts.AddPolicy("test", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("user", "admin");
                    policy.RequireClaim("test", "foobar");
                    policy.RequireUserName("some@email.com");
                    //policy.RequireAssertion(context => context.Resource)
                });
                opts.AddPolicy("age", policy =>
                {
                    policy.Requirements.Add(new AgeRequirement(18));
                });
            });

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddScoped<IRequestStoreService, RequestStoreService>();
            services.AddSingleton<IApplicationService, ApplicationService>(provider => new ApplicationService("MVC Test Application"));
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IDapperService, DapperService>(x=> new DapperService(connection));

            services.Configure<SomeSettings>(Configuration.GetSection("SomeSettings"));


            services.Configure<RouteOptions>(opts =>
            {
                opts.ConstraintMap.Add("myExists", typeof(MyKnownRouteValueConstraint));
            });
            services.AddControllersWithViews(opts =>
            {
                opts.ValueProviderFactories.Add(new CookieValueProviderFactory());
                opts.ModelBinderProviders.Insert(0, new CustomDateTimeModelBinderProvider());

                opts.Filters.Add<LastVisitResourceFilter>();
                //var cacheFilter = new ResponseCacheAttribute { Duration = 30 };
                //opts.Filters.Add(cacheFilter);
            });//.AddRazorRuntimeCompilation();

            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddSignalR(opts =>
                {
                    //opts.ClientTimeoutInterval = TimeSpan.FromSeconds(5);
                    //opts.HandshakeTimeout = TimeSpan.FromSeconds(5);
                    opts.KeepAliveInterval = TimeSpan.FromSeconds(60);
                    opts.EnableDetailedErrors = true;
                })
                .AddHubOptions<ChatHub>(opts => { });
            //services.AddResponseCompression(opts=>
            //{
            //    opts.EnableForHttps = true;
            //});
            //services.Configure<BrotliCompressionProviderOptions>(options =>
            //{
            //    options.Level = CompressionLevel.Optimal;
            //});

            services.AddHostedService<StartupHostedService>();
            services.AddSingleton<StartupHostedServiceHealthCheck>();

            services.AddHealthChecks()
                .AddCheck<ExampleHealthCheck>("example_health_check", HealthStatus.Unhealthy, new[] {"tag1"})
                //.AddCheck("failed_check", () => HealthCheckResult.Unhealthy("Fooo"), new[] {"tag2"})
                .AddSqlServer(connection, name: "sql_server")
                .AddDbContextCheck<TestappdbContext>("ef_check")
                .AddCheck<StartupHostedServiceHealthCheck>("hosted_service_check", HealthStatus.Degraded, new []{ "ready" });

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(2);
                //options.Predicate = (check) => check.Tags.Contains("ready");
            });

            services.AddSingleton<IHealthCheckPublisher, ReadinessPublisher>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseMiddleware<PerformanceMiddleware>();

            app.UseMiddleware<RequestStoreTestMiddleware>();

            //app.UseResponseCompression();

            app.UseSession();

            app.Use(async (context, next) =>
            {
                if (!context.Session.IsAvailable)
                {
                    logger.LogWarning("Session is NOT awailable");
                    await next();
                    return;
                }

                if (!context.Session.Keys.Contains("_myVal"))
                {
                    context.Session.SetString("_myVal", "fooBar");
                }
                await next();
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                
                //app.UseHsts();
            }
            
            var rewriteOptions = new RewriteOptions();
            ////https://localhost:/homeindex => https://localhost:/index-home
            //rewriteOptions.AddRedirect("^(home)(index)$","$2-$1");
            ////better to use app.UseHttpsRedirection()
            //rewriteOptions.AddRedirectToHttps((int)HttpStatusCode.TemporaryRedirect, 44324);
            ////Template for redirectiong from www domain
            //rewriteOptions.AddRedirectToNonWww();
            ////Template for redirectiong to www domain
            //rewriteOptions.AddRedirectToWww();
            rewriteOptions.AddRewrite("(?i)^privacy$", "Home/Privacy", true);

            app.UseRewriter(rewriteOptions);

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //Маршрутизация областей: (работают все три варианта)
                endpoints.MapControllerRoute("areas", "{area:myExists}/{controller=Home}/{action=Index}/{id?}");
                //MapAreaControllerRoute лучше использовать для определенных областей и маршрутов, для стандартного маршрута лучше использовать MapControllerRoute
                //endpoints.MapAreaControllerRoute("cabinet", "Cabinet", "{area:myExists}/{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapAreaControllerRoute("cabinet", "Cabinet", "cabinet/{controller=Home}/{action=Index}/{id?}");//полезно, если маршрут не совпадает с названием области

                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute("test", "testLink{foo}-{bar}", new { controller = "Home", action = "Index"});

                endpoints.MapHub<ChatHub>("/chat", opts => { });
                endpoints.MapHub<NotificationHub>("/notify", opts => { });

                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("tag1")
                });
                endpoints.MapHealthChecks("/healthsql", new HealthCheckOptions
                {
                    Predicate = check => check.Name == "sql_server"
                });
                endpoints.MapHealthChecks("/health2", new HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    //Predicate = check => check.Name == "failed_check",
                    ResultStatusCodes = new Dictionary<HealthStatus, int>
                    {
                        {HealthStatus.Healthy, StatusCodes.Status200OK},
                        {HealthStatus.Degraded, StatusCodes.Status200OK},
                        {HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable}
                    },
                    ResponseWriter = JsonWriter
                });
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = _ => false
                });
            });
        }

        private static Task JsonWriter (HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", 
                    new JObject(result.Entries.Select(pair =>
                        new JProperty(pair.Key, new JObject(
                            new JProperty("status", pair.Value.Status.ToString()),
                            new JProperty("description", pair.Value.Description),
                            new JProperty("data", 
                                new JObject(pair.Value.Data.Select(p => new JProperty(p.Key, p.Value))
                            ))))))));

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }
}
