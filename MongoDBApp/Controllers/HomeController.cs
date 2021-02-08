using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDBApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDBApp.Models.db;

namespace MongoDBApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsersService _db;

        public HomeController(ILogger<HomeController> logger, UsersService db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index(string name)
        {
            IEnumerable<User> users = await _db.GetUsers(name);

            var model = new IndexViewModel
            {
                Users = users,
                Name = name
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _db.Create(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(string id)
        {
            User user = await _db.GetUser(id);
            if (user == null) return RedirectToAction("Index");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _db.Update(model);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _db.Remove(id);

            return RedirectToAction("Index");
        }
    }
}
