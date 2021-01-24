using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace IdentitySandboxApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            InitData(scope.ServiceProvider).Wait();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static async Task InitData(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();

            if (!await roleManager.RoleExistsAsync("admin"))
            {
                var role = new IdentityRole<long>("admin");
                await roleManager.CreateAsync(role);
            }

            if (!await roleManager.RoleExistsAsync("user"))
            {
                var role = new IdentityRole<long>("user");
                await roleManager.CreateAsync(role);
            }

            var userManager = services.GetRequiredService<UserManager<User>>();

            var admin = await userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                var user = new User {
                    DateOfBirth = DateTime.MinValue,
                    DateOfRegistration = DateTime.Now,
                    Email = "admin@email.com",
                    EmailConfirmed = true,
                    UserName = "admin"
                };

                await userManager.CreateAsync(user, "qwerty");
            }

            var someUser = await userManager.FindByNameAsync("someUser");
            if (someUser == null)
            {
                var user = new User
                {
                    DateOfBirth = new DateTime(1990, 02, 17),
                    DateOfRegistration = DateTime.Now,
                    Email = "someUser@email.com",
                    EmailConfirmed = true,
                    UserName = "someUser"
                };

                await userManager.CreateAsync(user, "qwerty");
            }
        }
    }
}
