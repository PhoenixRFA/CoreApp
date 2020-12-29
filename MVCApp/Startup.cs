using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCApp.Constraints;
using MVCApp.Middleware;
using MVCApp.Models;
using MVCApp.Services;

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
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddScoped<IRequestStoreService, RequestStoreService>();
            services.AddSingleton<IApplicationService, ApplicationService>(provider => new ApplicationService("MVC Test Application"));

            services.Configure<SomeSettings>(Configuration.GetSection("SomeSettings"));


            services.Configure<RouteOptions>(opts =>
            {
                opts.ConstraintMap.Add("myExists", typeof(MyKnownRouteValueConstraint));
            });
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<PerformanceMiddleware>();

            app.UseMiddleware<RequestStoreTestMiddleware>();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                
                //app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                //ћаршрутизаци€ областей: (работают все три варианта)
                //endpoints.MapControllerRoute("cabinet", "{area:myExists}/{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapAreaControllerRoute("cabinet", "Cabinet", "{area:myExists}/{controller=Home}/{action=Index}/{id?}");
                //endpoints.MapAreaControllerRoute("cabinet", "Cabinet", "cabinet/{controller=Home}/{action=Index}/{id?}");//полезно, если маршрут не совпадает с названием области
            });
        }
    }
}
