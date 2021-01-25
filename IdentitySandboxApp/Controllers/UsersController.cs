using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentitySandboxApp.Models.Identity;
using IdentitySandboxApp.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySandboxApp.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authService;

        public UsersController(UserManager<User> userManager, IAuthorizationService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public IActionResult Index(string msg = null)
        {
            var model = new UsersIndexModel
            {
                Users = _userManager.Users.ToList()
            };

            ViewData["Message"] = msg;

            return View(model);
        }

        public async Task<IActionResult> UserDetails(long id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return UserNotFound();
            }

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

        [HttpGet]
        public IActionResult CreateUser() => View();
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Данный логин уже занят");
                return View(model);
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Данный Email уже занят");
                return View(model);
            }

            if (!DateTime.TryParse(model.DateOfBirth, out DateTime parcedDate))
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Не верный формат даты");
                return View(model);
            }

            var newUser = new User
            {
                UserName = model.Username,
                DateOfBirth = parcedDate,
                DateOfRegistration = DateTime.Now,
                Email = model.Email,
                PhoneNumber = model.Phone
            };

            IdentityResult res = await _userManager.CreateAsync(newUser, model.Password);

            if (!res.Succeeded)
            {
                foreach (IdentityError error in res.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Index", new {msg = "Новый пользователь создан"});
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(long id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return UserNotFound();
            }

            AuthorizationResult authResult = await _authService.AuthorizeAsync(User, user, new OperationAuthorizationRequirement { Name = "Delete" });
            
            var model = new EditUserModel
            {
                Id = user.Id,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd"),
                Phone = user.PhoneNumber,
                Username = user.UserName,
                CanDelete = authResult.Succeeded
            };

            ViewData["Title"] = $"Изменение пользователя - {user.UserName}";
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!DateTime.TryParse(model.DateOfBirth, out DateTime parcedDate))
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Не верный формат даты");
                return View(model);
            }

            User user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
            {
                return RedirectToAction("Index", new {msg = "Пользователь не найден"});
            }

            if (user.Email != model.Email && await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Данный Email уже занят");
                return View(model);
            }

            user.Email = model.Email;
            user.DateOfBirth = parcedDate;
            user.UserName = model.Username;
            user.PhoneNumber = model.Phone;

            IdentityResult res = await _userManager.UpdateAsync(user);

            if (!res.Succeeded)
            {
                foreach (IdentityError error in res.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Index", new {msg = "Изменения сохранены"});
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(long id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return UserNotFound();
            }
            
            ViewData["Title"] = $"Удалить пользователя - {user.UserName}";
            return View(id);
        }
        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteUser")]
        public async Task<IActionResult> DoDeleteUser(long id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return RedirectToAction("Index", new { msg = "Нельзя удалить admin'а" });
            }

            var authRes = await _authService.AuthorizeAsync(User, user, new OperationAuthorizationRequirement { Name = "Delete" });
            if (!authRes.Succeeded)
            {
                return UserNotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index", new {msg = $"Пользователь - {user.UserName} удален"});
        }

        [HttpGet]
        public IActionResult ChangeUserPassword(long id)
        {
            var model = new ChangeUserPasswordModel{ UserId = id };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserPassword(ChangeUserPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                return UserNotFound();
            }

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, model.Password);

            return RedirectToAction("Index", new {msg = "Пароль изменен"});
        }

        private IActionResult UserNotFound() => 
            RedirectToAction("Index", new {msg = "Пользователь не найден"});

        
    }
}
