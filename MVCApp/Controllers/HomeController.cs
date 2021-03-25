using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApp.Models;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using MVCApp.SignalR;

namespace MVCApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TestappdbContext _db;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IHubContext<NotificationHub, IClient> _notifyHub;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, TestappdbContext db, IHubContext<ChatHub> chatHub, IHubContext<NotificationHub, IClient> notifyHub, IConfiguration config)
        {
            _logger = logger;
            _db = db;
            _chatHub = chatHub;
            _notifyHub = notifyHub;
            _config = config;
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

        public IActionResult Signalr()
        {
            return View();
        }

        public IActionResult SignalrTest()
        {
            _chatHub.Clients.All.SendAsync("send", "ATTENTION!", "system");
            return RedirectToAction("Index");
        }

        public IActionResult SignalrNotification(string msg)
        {
            _notifyHub.Clients.All.Notify(msg);
            return RedirectToAction("Index");
        }

        public IActionResult Fetch()
        {
            return View();
        }

        public IActionResult Data(string id)
        {
            Response.Cookies.Append("_foo", "fooval");
            Response.Headers.Add("_bar", "barval");

            return Json(new {
                id,
                method = Request.Method,
                items = new[] { "foo", "bar", "baz" }
            });
        }

        public IActionResult Err()
        {
            return StatusCode(500);
        }

        public IActionResult BadReq()
        {
            return BadRequest();
        }

        public IActionResult WebComponents()
        {
            return View();
        }

        public IActionResult ServiceWorker()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SavePushSubscription([FromBody]PushSubscription subscription)
        {
            if(subscription == null) return Json(new {result = false});

            _config["webPushKeys:endpoint"] = subscription.Endpoint;
            //_config["webPushKeys:auth"] = string.Join(string.Empty, subscription.Keys.Auth);
            //_config["webPushKeys:p256dh"] = string.Join(string.Empty, subscription.Keys.P256DH);

            return Json(new {result = true});
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
