using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MVCApp.Models;
using MVCApp.Services;

namespace MVCApp.Controllers
{
    public class CacheController : Controller
    {
        private readonly UsersService _db;
        private readonly IMemoryCache _cache;

        public CacheController(UsersService db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public IActionResult Index()
        {
            List<User> users = _db.GetUsers();

            var model = new CacheIndexModel
            {
                Users = users,
                IsFromCache = false
            };

            return View(model);
        }

        public IActionResult GetData(int id)
        {
            User user = _db.GetUser(id, out bool isFromCache);

            var model = new CacheGetDataModel
            {
                User = user,
                IsFromCache = isFromCache
            };
            
            return View(model);
        }

        public IActionResult SetCache()
        {
            User user = _cache.Set(1, new User());

            return Content(user.ToString());
        }
    }
}
