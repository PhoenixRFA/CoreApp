using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApp.Models;
using System.Diagnostics;
using System.Linq;

namespace MVCApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TestappdbContext _db;

        public HomeController(ILogger<HomeController> logger, TestappdbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult InitDb()
        {
            _db.Companies.AddRange(
                new Company
                {
                    Name = "Evil Company"
                }, new Company
                {
                    Name = "Good Company"
                }
            );

            _db.Languages.AddRange(
                new Language
                {
                    Name = 123
                }, new Language
                {
                    Name = 456
                }
            );

            _db.Roles.AddRange(
                new Role
                {
                    Name = "user"
                }, new Role
                {
                    Name = "admin"
                }
            );

            _db.SaveChanges();

            _db.Users.AddRange(
                new User
                {
                    Company = _db.Companies.First(),
                    Email = "admin@email.com",
                    Age = 27,
                    Password = "qwerty",
                    Sex = "male",
                    UserData = new UserDatum
                    {
                        Additional = "some info"
                    },
                    Name = "admin",
                    IsMarried = true,
                    Role = _db.Roles.First(x=>x.Name == "admin")
                },
                new User
                {
                    Company = _db.Companies.First(),
                    Email = "user@email.com",
                    Age = 23,
                    Password = "qwerty",
                    Sex = "male",
                    UserData = new UserDatum
                    {
                        Additional = "some info"
                    },
                    Name = "user",
                    IsMarried = false,
                    Role = _db.Roles.First(x=>x.Name == "user")
                }  
            );

            _db.SaveChanges();
            
            _db.LanguageUsers.AddRange(
                new LanguageUser
                {
                    Users = _db.Users.First(x=>x.Name == "admin"),
                    Languages = _db.Languages.First()
                },
                new LanguageUser
                {
                    Users = _db.Users.First(x=>x.Name == "user"),
                    Languages = _db.Languages.First()
                }    
            );

            _db.SaveChanges();

            return Content("Inited");
        }
    }
}
