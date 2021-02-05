using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MVCApp.Models;
using MVCApp.Services;

namespace MVCApp.Controllers
{
    public class CacheController : Controller
    {
        private readonly IUsersService _db;
        private readonly IMemoryCache _cache;

        public CacheController(IUsersService db, IMemoryCache cache)
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
            if (id <= 0)
            {
                return BadRequest($"{nameof(id)} must be > 0");
            }

            User user = _db.GetUser(id, out bool isFromCache);

            if (user == null)
            {
                return NotFound($"user with id: ${id} is not found");
            }

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

        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Client)]
        public IActionResult GetTime1()
        {
            return Content(DateTime.Now.ToString());
        }

        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.None)]
        public IActionResult GetTime2()
        {
            return Content(DateTime.Now.ToString());
        }

        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any, NoStore = true)]
        public IActionResult GetTime3()
        {
            return Content(DateTime.Now.ToString());
        }
    }
}
