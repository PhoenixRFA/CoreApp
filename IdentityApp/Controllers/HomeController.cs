using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> TestIdentity()
        {
            var sb = new StringBuilder();

            var newUser = new IdentityUser {
                Email = "email@email.com",
                PhoneNumber = "79876543211",
                UserName = "testUser"
            };
            IdentityResult res = await _userManager.CreateAsync(newUser, "Qwerty_1");
            string u = JsonConvert.SerializeObject(newUser);
            sb.AppendLine($"new user: {u}");

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            sb.AppendLine($"confirm email with token: {token}");
            await _userManager.ConfirmEmailAsync(newUser, token);

            string newEmail = "new@email.com";
            token = await _userManager.GenerateChangeEmailTokenAsync(newUser, newEmail);
            sb.AppendLine($"change email to {newEmail} with token: {token}");
            await _userManager.ChangeEmailAsync(newUser, newEmail, token);

            sb.AppendLine($"change pass without token, but with old one");
            await _userManager.ChangePasswordAsync(newUser, "Qwerty_1", "NewPass_1");
            bool isPassOk = await _userManager.CheckPasswordAsync(newUser, "NewPass_1");
            sb.AppendLine($"check new password: {isPassOk}");

            await _userManager.AccessFailedAsync(newUser);
            sb.AppendLine($"increase access failed");
            int failedCount = await _userManager.GetAccessFailedCountAsync(newUser);
            sb.AppendLine($"count: {failedCount}");

            int recoveryCodes = await _userManager.CountRecoveryCodesAsync(newUser);
            sb.AppendLine($"recovery codes count: {recoveryCodes}");

            IdentityUser usr = await _userManager.FindByNameAsync("testUser");
            u = JsonConvert.SerializeObject(usr);
            sb.AppendLine($"user: {u}");

            sb.AppendLine($"delete user");
            await _userManager.DeleteAsync(newUser);

            return Content(sb.ToString());
        }

        public async Task<IActionResult> PassCheck()
        {
            var user = await _userManager.FindByNameAsync("some@email.com");
            
            var _passwordValidator = HttpContext.RequestServices.GetService(typeof(IPasswordValidator<IdentityUser>)) as IPasswordValidator<IdentityUser>;
            var _passwordHasher = HttpContext.RequestServices.GetService(typeof(IPasswordHasher<IdentityUser>)) as IPasswordHasher<IdentityUser>;
            
            IdentityResult result = await _passwordValidator.ValidateAsync(_userManager, user, "Qwerty_1");
            string hash = _passwordHasher.HashPassword(null, "Qwerty_1");

            return Content($"user: {user.Email} \n\rpassw: {user.PasswordHash}\n\rnew pass: Qwerty_1\n\nnew pass hash: {hash}");
        }

        //public async Task<IActionResult> AddRole()
        //{
        //    var user = await _userManager.FindByNameAsync("some@email.com");

        //    if(!_roleManager.Roles.Any(x => x.Name == "manager")){
        //        await _roleManager.CreateAsync(new IdentityRole { 
        //            Name = "manager"
        //        });
        //    }

        //    await _userManager.AddToRoleAsync(user, "manager");
        //    bool inRole1 = await _userManager.IsInRoleAsync(user, "manager");

        //    return Content("ok");
        //}

        //public async Task<IActionResult> RemoveFromRole()
        //{
        //    var user = await _userManager.FindByNameAsync("some@email.com");
        //    await _userManager.RemoveFromRoleAsync(user, "manager");

        //    return Content("ok");
        //}

        //[Authorize(Roles = "manager")]
        //public async Task<IActionResult> RoleCheck()
        //{
        //    var user = await _userManager.FindByNameAsync("some@email.com");
        //    IList<string> roles = await _userManager.GetRolesAsync(user);

        //    return Content(string.Join(", ", roles));
        //}
    }
}
