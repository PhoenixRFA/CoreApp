using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MVCApp.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _appEnvironment;

        public UploadController(IWebHostEnvironment environment)
        {
            _appEnvironment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null) return RedirectToAction("Index");

            string folder = Path.Combine(_appEnvironment.WebRootPath, "files");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            await using var stream = new FileStream(Path.Combine(_appEnvironment.WebRootPath, "files", file.FileName), FileMode.Create);
            await file.CopyToAsync(stream);

            return RedirectToAction("Index");
        }

        static MemoryStream _ms = null;

        [HttpPost]
        public async Task<IActionResult> UploadChunks(IFormFile data, string name, string mime, bool isLast = false)
        {
            _ms ??= new MemoryStream();

            await data.CopyToAsync(_ms);

            if (isLast)
            {
                _ms.Seek(0, SeekOrigin.Begin);
                await using var stream = new FileStream(Path.Combine(_appEnvironment.WebRootPath, "files", name), FileMode.Create);
                await _ms.CopyToAsync(stream);

                _ms = null;
            }

            return Content("");
        }
    }
}
