using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Controllers
{
    public class CabinetController : Controller
    {
        public IActionResult Home(string id)
        {
            return Content("Oops! This is CabinetController, Home action");
        }
    }
}
