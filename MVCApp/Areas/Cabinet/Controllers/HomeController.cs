using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Areas.Cabinet.Controllers
{
    [Area("cabinet")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
