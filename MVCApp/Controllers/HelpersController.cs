using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVCApp.Models;

namespace MVCApp.Controllers
{
    public class HelpersController : Controller
    {
        public IActionResult Index()
        {
            var model = new HelpersIndexViewModel
            {
                Values = new List<string> {"ItemA", "ItemB", "ItemC", "ItemD"}
            };
            
            return View(model);
        }
    }
}
