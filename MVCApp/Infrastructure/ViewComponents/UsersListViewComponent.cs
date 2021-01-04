using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MVCApp.Infrastructure.ViewComponents
{
    public class UsersListViewComponent : ViewComponent
    {
        private List<string> _users = new List<string>{"Bill", "Bob", "Jeb"};

        public IViewComponentResult Invoke()
        {
            return View(_users);
        }
    }
}
