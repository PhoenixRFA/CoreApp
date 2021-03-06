﻿using IdentitySandboxApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace IdentitySandboxApp.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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

        
        public IActionResult AntiforgeryTest()
        {
            return Content("ModelState: " + ModelState.IsValid);
        }

        [EnableCors("cors_test")]
        public IActionResult CorsTest()
        {
            return Content("Ok");
        }

        [Authorize]
        public IActionResult SignalR()
        {
            return View();
        }
    }
}
