using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesApp.Pages
{
    public class UsersModel : PageModel
    {
        //[BindProperty(Name = "name", SupportsGet = true)]
        //public string Name { get; set; }
        //[BindProperty] public int? Age { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        [BindProperty(SupportsGet = true)] public UserModel Person { get; set; }


        public bool IsCorrect { get; set; } = true;
        public string Message { get; set; }
        public List<UserModel> Users = UsersDb;

        public static List<UserModel> UsersDb { get; set; } = new List<UserModel>(2);

        public void OnGet()
        {
            string filter = $"(Filter: ID:{Id})"; //; Name:{Person?.Name}; Age:{Person?.Age};

            Message = "Fill the form" + filter;
        }

        public void OnGetByName(string name)
        {
            Users = UsersDb.Where(x => x.Name == name).ToList();

            Message = $"Filtered by name: {name}";
        }

        public IActionResult OnGetForbidden()
        {
            return StatusCode((int)HttpStatusCode.Forbidden);
        }

        public void OnPost()
        {
            //if (Age == null || Age < 1 || Age > 110 || string.IsNullOrEmpty(Name))
            //{
            //    IsCorrect = false;
            //    Message = "Wrong data!";
            //}
            //else
            //{
            //    Message = "Processed";
            //    Users.Add(new UserModel
            //    {
            //        Age = Age.Value,
            //        Name = Name
            //    });
            //}

            if (Person.Age < 1 || Person.Age > 110 || string.IsNullOrEmpty(Person.Name))
            {
                IsCorrect = false;
                Message = "Wrong data!";
            }
            else
            {
                Message = "Processed";
                UsersDb.Add(Person);
            }
        }

        public class UserModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }


}
