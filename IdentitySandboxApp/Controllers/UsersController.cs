using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentitySandboxApp.Models.Identity;
using IdentitySandboxApp.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySandboxApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var model = new UsersIndexModel
            {
                Users = _userManager.Users.ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> UserDetails(long id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            IList<string> roles = await _userManager.GetRolesAsync(user);

            string userRoles = roles.Any() ? string.Join(", ", roles) : "—";

            var model = new UserDetailsModel
            {
                User = user,
                UserRoles = userRoles
            };
            ViewData["Title"] = $"Пользователь - {user.UserName}";

            return View(model);
        }
    }
}
