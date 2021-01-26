using IdentitySandboxApp.Data;
using IdentitySandboxApp.Infrastructure;
using IdentitySandboxApp.Infrastructure.AuthHandlers;
using IdentitySandboxApp.Infrastructure.AuthMiddleware;
using IdentitySandboxApp.Infrastructure.ClaimsFactory;
using IdentitySandboxApp.Models;
using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySandboxApp
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
            //services.AddTransient<IEmailSender, DebugEmailSender>();
            services.AddTransient<IEmailSender, MailKitEmailSender>();

            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                //var builder = new SqlConnectionStringBuilder(Configuration.GetConnectionString("DefaultConnection"))
                //{
                //    Password = Configuration.GetSection("dbConnection").Value
                //};
                //opts.UseSqlServer(builder.ConnectionString);

                opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            });
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<TestClaimsFactory>()
                .AddTokenProvider<DigitsTokenProvider>("Digits");
            //.AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);
            services.AddControllersWithViews();

            services.AddAuthentication()
                .AddJwtBearer(opts =>
                {
                    //opts.ForwardDefault = "Identity.Application";
                    opts.ForwardAuthenticate = "Identity.Application";

                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,

                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = AuthOptions.GetKey()
                    };
                })
                .AddGoogle(opts =>
                {
                    IConfigurationSection credentials = Configuration.GetSection("Auth:Google");

                    opts.ClientId = credentials["ClientId"];
                    opts.ClientSecret = credentials["ClientSecret"];
                });

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("IsAdmin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireUserName("admin");
                });
            });

            services.AddCors(opts =>
            {
                opts.AddPolicy("cors_test", builder =>
                {
                    builder.WithOrigins("http://localhost:54910");
                    builder.AllowAnyHeader();
                    builder.WithMethods("post");
                });
            });
            
            services.AddScoped<IAuthorizationHandler, UserManagerAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, TestAuthHandler>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthMiddleware>();

            //services.AddDataProtection();

            services.Configure<IdentityOptions>(opts =>
            {
                opts.Password.RequireDigit = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequiredLength = 6;
                opts.Password.RequiredUniqueChars = 1;

                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";

                opts.SignIn.RequireConfirmedAccount = true;

                opts.Lockout.MaxFailedAccessAttempts = 2;

                opts.Tokens.ChangeEmailTokenProvider = "Digits";
                opts.Tokens.EmailConfirmationTokenProvider = "Email";
                opts.Tokens.ChangePhoneNumberTokenProvider = "Phone";
                opts.Tokens.PasswordResetTokenProvider = "Authenticator";
            });
            services.Configure<PasswordHasherOptions>(opts =>
            {
                opts.IterationCount = 12000;
                opts.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            });
            services.ConfigureApplicationCookie(opts =>
            {
                opts.Cookie.Name = "IdentityCookie";
            });

            //Upadte impersonating cookies on cookie refresh
            services.Configure<SecurityStampValidatorOptions>(opts =>
            {
                opts.OnRefreshingPrincipal = context =>
                {
                    var originalUserIdClaim = context.CurrentPrincipal.FindFirst("OriginalUserId");
                    var isImpersonatingClaim = context.CurrentPrincipal.FindFirst("IsImpersonating");

                    if (isImpersonatingClaim?.Value == true.ToString() && originalUserIdClaim?.Value != null)
                    {
                        context.NewPrincipal.Identities.First().AddClaims(new[] { originalUserIdClaim, isImpersonatingClaim });
                    }

                    return Task.CompletedTask;
                };
            });

            services.Configure<AntiforgeryOptions>(opts =>
            {
                opts.FormFieldName = "_af";
                opts.HeaderName = "H_antiforgery";
                //Если true - в ответ на запрос будет передаваться заголовок X-Frame-Options = SAMEORIGIN
                bool val = opts.SuppressXFrameOptionsHeader;
                //opts.Cookie.Name = "COOKIE_af";
            });

            services.Configure<SmtpOptions>(Configuration.GetSection("smtpSettings"));

            services.Configure<HstsOptions>(opts =>
            {
                opts.Preload = true;
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromDays(365);
            });

            services.Configure<CookiePolicyOptions>(opts =>
            {
                opts.CheckConsentNeeded = context => true;
                opts.MinimumSameSitePolicy = SameSiteMode.None;
                opts.Secure = CookieSecurePolicy.None;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
