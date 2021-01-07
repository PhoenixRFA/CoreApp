using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFCoreApp.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EFCoreApp
{
    class Program
    {
        private static DbContextOptions _options;
        private static IConfiguration _config;

        public static readonly ILoggerFactory CustomLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new CustomLoggerProvider());
        });

        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile("appsettings.json");
            IConfigurationRoot config = configBuilder.Build();
            _config = config;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

            string connectionString = _config.GetConnectionString("DefaultConnection");

            DbContextOptions<ApplicationContext> options = optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer(connectionString)
                .LogTo(x =>
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(x);
                    Console.ForegroundColor = color;
                }, new[] { RelationalEventId.CommandExecuted }, LogLevel.Information)//, DbContextLoggerOptions.SingleLine)
                //.UseLoggerFactory(CustomLoggerFactory)
                .Options;

            _options = options;


            Console.ForegroundColor = ConsoleColor.White;


            //TestDb();
            //Relations();
            //Linq();
            Sql();
        }

        private static void TestDb()
        {
            Console.WriteLine("1. Test DB");

            using (var db = new ApplicationContext(_options))
            {
                var user1 = new User { Name = "Bob", Age = 25 };
                var user2 = new User { Name = "Bill", Age = 27 };

                db.Users.Add(user1);
                db.Add(user2);
                Console.WriteLine("Users added");

                db.SaveChanges();
                Console.WriteLine("Save changes");

                ShowUsers(db);
            }

            User userToEditInNewContext;
            using (var db = new ApplicationContext(_options))
            {
                User userToEdit = db.Users.FirstOrDefault();
                userToEditInNewContext = db.Users.OrderBy(x=>x.Id).LastOrDefault();

                Console.WriteLine("Edit user");
                if (userToEdit != null)
                {
                    userToEdit.Age = 18;
                    userToEdit.Name = "Sergey";

                    db.SaveChanges();
                }
                else
                {
                    Console.WriteLine("No users to edit!");
                }

                ShowUsers(db);
            }

            using (var db = new ApplicationContext(_options))
            {
                Console.WriteLine("Update user from another context");
                if(userToEditInNewContext != null)
                {
                    userToEditInNewContext.Age = 33;
                    userToEditInNewContext.Name = "John";

                    db.Users.Update(userToEditInNewContext);
                    db.SaveChanges();
                }

                ShowUsers(db);
            }

            using (var db = new ApplicationContext(_options))
            {
                Console.WriteLine("Update user without tracking");
                User user = db.Users.AsNoTracking().FirstOrDefault();
                
                if (userToEditInNewContext != null)
                {
                    user.Age = 50;
                    userToEditInNewContext.Name = "Arthur";

                    db.Users.Update(user);
                    db.SaveChanges();
                }

                ShowUsers(db);
            }

            using (var db = new ApplicationContext(_options))
            {
                var userToDelete = db.Users.FirstOrDefault();

                Console.WriteLine("Delete user");
                if(userToDelete != null)
                {
                    db.Users.Remove(userToDelete);
                    db.SaveChanges();
                }
                else
                {
                    Console.WriteLine("No users to delete!");
                }

                ShowUsers(db);
            }
        }

        private static void Relations()
        {
            Console.WriteLine("2. Relations");

            using (var db = new ApplicationContext(_options))
            {
                var users = db.Users.Where(x => x.IsMarried);

                var u = users.FirstOrDefault();
                Console.WriteLine($"{u.Id}.{u.Name} - {u.Age}; ({u.Sex})");
                var data = u.UserData;
                Console.WriteLine($"Additional: {data?.Additional}");
                var company = u.Company;
                Console.WriteLine($"Company: {company?.Id}.{company?.Name}");
                var languages = u.Languages?.ToList() ?? new List<Language>(0);
                var langs = languages.Select(x => $"{x?.Id}.{x?.Name}");
                Console.WriteLine($"Langueges: {string.Join(", ", langs)}");
            }

            Console.WriteLine("\nLoad. Press any key\n");
            Console.ReadKey();

            using (var db = new ApplicationContext(_options))
            {
                var users = db.Users.Where(x => x.IsMarried);
                int id = users.Select(x => x.Id).FirstOrDefault();

                var u = users.FirstOrDefault();
                //db.Companies.Load();
                //db.UserData.Load();
                db.Entry(u).Reference(x => x.Company).Load();
                db.Entry(u).Reference(x => x.UserData).Load();
                db.Entry(u).Collection(x => x.Languages).Load();
                //db.Languages.Where(x=>x.Users.Any(us=>us.Id == id)).Load();


                Console.WriteLine($"{u.Id}.{u.Name} - {u.Age}; ({u.Sex})");
                var data = u.UserData;
                Console.WriteLine($"Additional: {data?.Additional}");
                var company = u.Company;
                Console.WriteLine($"Company: {company?.Id}.{company?.Name}");
                var languages = u.Languages?.ToList() ?? new List<Language>(0);
                var langs = languages.Select(x => $"{x?.Id}.{x?.Name}");
                Console.WriteLine($"Langueges: {string.Join(", ", langs)}");
            }

            Console.WriteLine("\nInclude. Press any key\n");
            Console.ReadKey();

            using (var db = new ApplicationContext(_options))
            {
                var users = db.Users
                    .Include(x=>x.Languages)
                    .Include(x=>x.Company)
                    .Include(x=>x.UserData)
                    .Where(x => !x.IsMarried).ToList();

                var u = users.FirstOrDefault();
                Console.WriteLine($"{u.Id}.{u.Name} - {u.Age}; ({u.Sex})");
                var data = u.UserData;
                Console.WriteLine($"Additional: {data.Additional}");
                var company = u.Company;
                Console.WriteLine($"Company: {company.Id}.{company.Name}");
                var languages = u.Languages.ToList();
                var langs = languages.Select(x => $"{x.Id}.{x.Name}");
                Console.WriteLine($"Langueges: {string.Join(", ", langs)}");
            }
        }

        private static void Linq()
        {
            Console.WriteLine("3. Linq");

            using (var db = new ApplicationContext(_options))
            {
                var users = db.Users.Where(x => EF.Functions.Like(x.Name, "B%")).ToList();

                Console.WriteLine("Where name Like B%");
                foreach (var u in users)
                {
                    Console.WriteLine($"{u.Id}.{u.Name}");
                }

                User u1 = db.Users.FirstOrDefault();
                User u2 = db.Users.FirstOrDefault();

                Console.WriteLine($"u1 == u2 ({u1 == u2})");

                Console.WriteLine($"Entries count: {db.ChangeTracker.Entries().Count()}");
            }
        }

        private static void Sql()
        {
            using (var db = new ApplicationContext(_options))
            {
                List<User> res = db.Users.FromSqlRaw("SELECT * FROM Users").ToList();

                foreach (User u in res)
                {
                    Console.WriteLine($"{u.Id}.{u.Name} - {u.Age}");
                }

                int count = db.Database.ExecuteSqlRaw("SELECT * FROM Users");
                Console.WriteLine($"count = {count}");
            }
        }

        private static void ShowUsers(ApplicationContext db)
        {
            List<User> users = db.Users.ToList();
            Console.WriteLine("Users:");
            foreach (var user in users)
            {
                Console.WriteLine($"\t{user.Id}.{user.Name} - {user.Age}");
            }
        }
    }
}
