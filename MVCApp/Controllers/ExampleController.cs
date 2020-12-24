using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace MVCApp.Controllers
{
    public class ExampleController : Controller
    {
        private readonly ILogger<ExampleController> _logger;

        public ExampleController(ILogger<ExampleController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [ActionName("String")]
        public string GetString() => "String result";

        [NonAction]
        public string NotAction() => "Cant access via route";

        [HttpGet]
        public string Query1(string val) => $"GET: {val}";
        [HttpPost]
        public string Query2(string val) => $"POST: {val}";
        [HttpPut]
        public string Query3(string val) => $"PUT: {val}";
        [HttpDelete]
        public string Query4(string val) => $"DELETE: {val}";

        //URL: /example/getarray?array=1&array=2&array=3 OR /example/getarray?array[0]=1&array[2]=2&array[1]=3
        public string GetArray(int[] array) => $"{string.Join(" + ", array)} = {array.Sum()}";

        public object GetObject() => new {foo = 1, bar = new {a = "qwe", b = 3.14}};

        public IActionResult Content() => Content("Content result");
        
        //Status: 200
        public IActionResult Empty() => new EmptyResult();
        //Status: 204
        public IActionResult NoContentRes() => NoContent();
        public IActionResult FileRes()
        {
            string path = @"C:\Users\Phoenix\source\repos\CoreApp\MVCApp\wwwroot\favicon.ico";
            string mime = "image/x-icon";
            FileStream stream = System.IO.File.OpenRead(path);
            
            //return File(stream, mime, "icon.ico");
            //return PhysicalFile(path, mime, "myicon.ico");
            return File("~/favicon.ico", mime);
        }

        public IActionResult Json()
        {
            return Json(new {foo = 123, bar = new { a = 1, b = 2}});
        }

        public IActionResult Redirect()
        {
            return RedirectToAction("RedirectTarget");
        }

        public IActionResult RedirectTarget()
        {
            return Content("Redirected");
        }

        public IActionResult Forbidden()
        {
            return Content("Forbidden");
        }

        public IActionResult Log(string msg,[FromServices] ILogger<ExampleController> logger)
        {
            logger.LogInformation(msg);
            return Content($"Message '{msg}' logged");
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Debug.WriteLine("OnActionExecuting");
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Debug.WriteLine("OnActionExecuted");
        }

        //Предотвращает вызовы OnActionExecuting и OnActionExecuted
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //подобно OnActionExecuting
            Debug.WriteLine("OnActionExecutionAsync Before");

            if ((string) context.RouteData.Values["action"] == "Forbidden")
            {
                _logger.LogInformation("Access to Forbidden");
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();

            //подобно OnActionExecuted
            Debug.WriteLine("OnActionExecutionAsync After");
            //return Task.CompletedTask;
        }
    }
}
