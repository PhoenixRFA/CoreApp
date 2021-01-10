using IdentityApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;

namespace IdentityApp
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
            //connecting with database via EntityFramework
            services.AddDbContext<ApplicationDbContext>(options =>
                //use SQL Server driver
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //перехватывает исключения, относящиеся к базе данных, которые могут быть устранены с помощью миграций Entity Framework
            services.AddDatabaseDeveloperPageExceptionFilter();

            //Подключает все необходимые сервисы для стандартного Identity
            /* services.AddAuthentication(o => {
             *  o.DefaultScheme = IdentityConstants.ApplicationScheme; //Identity.Application
             *  o.DefaultSignInScheme = IdentityConstants.ExternalScheme; //Identity.External
             * })
             * .AddIdentityCookies(o => {}); //AddCookie()
             * services.AddIdentityCore() //подключает необходимые сервисы (scoped) для Identity (UserValidator, PasswordValidator, PasswordHasher, UpperInvariantLookupNormalizer, DefaultUserConfirmation, IdentityErrorDescriber, UserClaimsPrincipalFactory, UserManager)
             * .AddDefaultUI() //подключает UI по-умолчанию для Identity
             * .AddDefaultTokenProviders(); //добавляет провайдеры для генерации токенов двухфакторной авторизации, изменения пароля, и т.д.
             */
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //указывает какой контекст БД использовать
                .AddEntityFrameworkStores<ApplicationDbContext>();
            
            services.AddControllersWithViews();

            ConfigureIdentity(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                //Применяет миграцию для исправаления ошибок EF
                //Путь по-умолчанию: /ApplyDatabaseMigrations
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
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private void ConfigureIdentity(IServiceCollection services)
        {
            //All Identity Options
            services.Configure<IdentityOptions>(cfg =>
            {
                cfg.Password.RequireDigit = true;
                cfg.Password.RequireLowercase = true;
                cfg.Password.RequireNonAlphanumeric = false; //not-default
                cfg.Password.RequireUppercase = true;
                cfg.Password.RequiredLength = 6;
                cfg.Password.RequiredUniqueChars = 1;
                
                //Default values
                cfg.Lockout.AllowedForNewUsers = true;
                cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                cfg.Lockout.MaxFailedAccessAttempts = 5;

                cfg.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                cfg.User.RequireUniqueEmail = true; //not-default

                //Default values
                cfg.Tokens.AuthenticatorIssuer = "Microsoft.AspNetCore.Identity.UI";
                cfg.Tokens.AuthenticatorTokenProvider = "Authenticator";
                cfg.Tokens.ChangeEmailTokenProvider = "Default";
                cfg.Tokens.ChangePhoneNumberTokenProvider = "Phone";
                cfg.Tokens.EmailConfirmationTokenProvider = "Default";
                cfg.Tokens.PasswordResetTokenProvider = "Default";
                //cfg.Tokens.ProviderMap = new Dictionary<string, TokenProviderDescriptor>();

                //Default values
                cfg.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
                cfg.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
                cfg.ClaimsIdentity.SecurityStampClaimType = "AspNet.Identity.SecurityStamp";
                cfg.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
                cfg.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;

                //Default values
                cfg.SignIn.RequireConfirmedAccount = true;
                cfg.SignIn.RequireConfirmedEmail = false;
                cfg.SignIn.RequireConfirmedPhoneNumber = false;

                //Default values
                cfg.Stores.MaxLengthForKeys = 128;
                cfg.Stores.ProtectPersonalData = false;
            });

            services.ConfigureApplicationCookie(cfg =>
            {
                cfg.Cookie.HttpOnly = true;
                cfg.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                cfg.LoginPath = "/identity/account/login";
                cfg.AccessDeniedPath = "/identity/account/accessdenied";
                cfg.LogoutPath = "/identity/account/logout";
                cfg.SlidingExpiration = true;
            });
        }
    }
}
