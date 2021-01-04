using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Areas.Cabinet.Controllers
{
    [Area("Cabinet")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
