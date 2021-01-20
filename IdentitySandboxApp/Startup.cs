using IdentitySandboxApp.Data;
using IdentitySandboxApp.Infrastructure;
using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

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
            services.AddTransient<IEmailSender, DebugEmailSender>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<User, Role>()
                //.AddJwtBearer()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<DigitsTokenProvider>("Digits");
                //.AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);
            services.AddControllersWithViews();

            services.AddAuthentication()
                .AddJwtBearer(opts =>
                {
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
