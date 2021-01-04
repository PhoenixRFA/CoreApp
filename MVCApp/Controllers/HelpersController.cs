using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        public IActionResult TagHelpers(int id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Form()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FormTarget(FormModel model)
        {
            bool isValid = ModelState.IsValid;

            var sb = new StringBuilder();
            foreach (KeyValuePair<string, ModelStateEntry> modelState in ViewData.ModelState)
            {
                foreach (ModelError error in modelState.Value.Errors)
                {
                    sb.AppendFormat("\t{0} = {1}\n\r", modelState.Key, error.ErrorMessage);
                }
            }

            //List<ModelErrorCollection> errors = ModelState.Values.Where(x => x.Errors.Any()).Select(x => x.Errors).ToList();
            
            return Content($"Model: {(isValid ? "valid" : "invalid")}\n\rName: {model.Name}\n\rSurname: {model.Surname}\n\rAge: {model.Age}\n\rEmail: {model.Email}\n\rPhone: {model.Phone}\n\rPassword: {model.Password}\n\r{sb}");
        }

        public IActionResult PasswordCheck(string password)
        {
            return Json(password != null && password.Length > 3 && !password.Contains("*"));
        }
    }

    public class FormModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Surname { get; set; }
        [Range(1,10)]
        public int Age { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Phone]
        public string Phone { get; set; }

        [Required, StringLength(12, MinimumLength = 6)]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string RepeatPassword { get; set; }
    }
}
