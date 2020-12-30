using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MVCApp.Infrastructure.ValueProviders;
using MVCApp.Models;
using MVCApp.Services;

namespace MVCApp.Controllers
{
    //С помощью аттрибута Route - можно задать альтернативный путь к контроллеру/действию
    //[Route("Example")]
    public class ExampleController : Controller
    {
        private readonly ILogger<ExampleController> _logger;
        private readonly IRequestStoreService _requestStore;
        private readonly IApplicationService _app;
        private readonly IDateTimeService _dt;

        private readonly IOptions<SomeSettings> _opts;
        private readonly IOptionsSnapshot<SomeSettings> _optsSnapshot;
        private readonly IOptionsMonitor<SomeSettings> _optsMonitor;

        public ExampleController(ILogger<ExampleController> logger, IApplicationService application, IRequestStoreService requestStore, IDateTimeService dateTime, IOptions<SomeSettings> options, IOptionsSnapshot<SomeSettings> optionsSnapshot, IOptionsMonitor<SomeSettings> optionsMonitor)
        {
            _logger = logger;
            _requestStore = requestStore;
            _app = application;
            _dt = dateTime;

            _opts = options;
            _optsSnapshot = optionsSnapshot;
            _optsMonitor = optionsMonitor;

            _optsMonitor.OnChange((opt, s) =>
            {
                _logger.LogInformation("Config {ConfigName} was changed ({DateTime}). {Additional}", opt.GetType().Name, DateTime.Now, s);
            });
        }

        //[Route("")]
        //[Route("Index")]
        public IActionResult Index()
        {
            string model = "Model is string";

            if (!TempData.ContainsKey("foo"))
            {
                TempData["foo"] = "Foo " + DateTime.Now;
            }

            TempData["bar"] = "Bar";
            
            return View(model: model);
        }
        
        public string GetTemp() => TempData["temp"]?.ToString() ?? "null";
        public string SetTemp(string id)
        {
            TempData["temp"] = id;
            return "ok";
        }

        public string DiTest()
        {
            _logger.LogInformation("Executed ExampleController.DiTest ({Date})", _dt.GetDateTime());
            return $"AppInfo: {_app.Name},{_app.ServerName} ({_app.Started})\n\rRequset ID: {_requestStore.Get("id")}\n\rWeek Start: {_dt.GetWeekStart()}\tToday: {_dt.GetDate()}\tWeek End: {_dt.GetWeekEnd()}";
        }

        public string ConfigurationTest()
        {
            _logger.LogInformation("Executed ExampleController.ConfigurationTest ({Date})", _dt.GetDateTime());
            return $"Options: {_opts.Value.Bar}, {_opts.Value.Foo}\n\rOptions Snapshot: {_optsSnapshot.Value.Bar}, {_optsSnapshot.Value.Foo}\n\rOptions Monitor: {_optsMonitor.CurrentValue.Bar}, {_optsMonitor.CurrentValue.Foo}";
        }
        
        public IActionResult GetCookies()
        {
            return Content(string.Join(", ", Request.Cookies.Keys));
        }
        
        //GET: /example/getcookie/session_cookie
        [HttpGet("{controller}/GetCookie/{name}")]
        public IActionResult GetCookie(string name)
        {
            string cookie = Request.Cookies[name];
            return Content(cookie);
        }
        //ANY: /example/setcookie/session_cookie/asdasd
        [Route("example/setCookie/{name}/{value}")]
        public IActionResult SetCookie(string name, string value)
        {
            Response.Cookies.Append(name, value);
            return RedirectToAction("GetCookies");
        }


        public IActionResult GetSessionData()
        {
            return Content(string.Join(", ", HttpContext.Session.Keys));
        }

        public IActionResult GetSession(string name)
        {
            string res = HttpContext.Session.GetString(name);
            return Content(res);
        }
        public IActionResult SetSession(string name, string value)
        {
            HttpContext.Session.SetString(name, value);
            return RedirectToAction("GetSessionData");
        }

        public string ValueProviderTest([FromCookie(Name = "cookie_test")] string cookieTest)
        {
            return $"ValueProviderTest: cookie_test = {cookieTest}";
        }
        public string ModelBinderTest(/*[FromCookie(Name = "date")]*/ DateTime dateTime)
        {
            return $"ModelBinderTest: DateTime = {dateTime}";
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

            if ((string)context.RouteData.Values["action"] == "Forbidden")
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
