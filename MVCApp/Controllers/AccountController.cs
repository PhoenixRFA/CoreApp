using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCApp.Infrastructure.AuthorizationRequirements;
using MVCApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCApp.Controllers
{
    public class AccountController : Controller
    {
        private TestappdbContext _db;
        
        public AccountController(TestappdbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _db.Users.Include(x=>x.Role).FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(model.Email, user.Role?.Name);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    Role userRole = _db.Roles.First(x => x.Name == "user");
                    _db.Users.Add(new User { Email = model.Email, Password = model.Password, Role = userRole });
                    await _db.SaveChangesAsync();

                    await Authenticate(model.Email, "user");

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Wrong Email or Password");
            }
            return View(model);
        }

        private async Task Authenticate(string userName, string roleName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, roleName ?? "user"),
                new Claim(ClaimTypes.DateOfBirth, "2003"),
                new Claim("test", "foobar")
            };
            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult Test()
        {
            bool isAuthenticated = User.Identity.IsAuthenticated;
            string name = User.Identity.Name;
            string authenticationType = User.Identity.AuthenticationType;

            Claim claim = User.Claims.First();
            ClaimsIdentity subj = claim.Subject;

            var claimsIdentity = User.Identity as ClaimsIdentity;
            claimsIdentity.AddClaim(new Claim("created", DateTime.Now.ToString(), ClaimValueTypes.DateTime, "CoreApp.MVCApp"));
            claimsIdentity.AddClaim(new Claim("id", "123", ClaimValueTypes.Integer, "CoreApp.MVCApp"));

            Claim idClaim = claimsIdentity.FindFirst(x => x.Type == "id");
            claimsIdentity.RemoveClaim(idClaim);
            bool has123Claim = claimsIdentity.HasClaim(x => x.Value == "123");

            return Content($"User is authenticated: {isAuthenticated}, name: {name}, authentication type: {authenticationType}"+
                $"\n\rClaim. Issuer: {claim.Issuer} (original issuer: {claim.OriginalIssuer}). Type: {claim.Type}. Value: {claim.Value} (Value type: {claim.ValueType}). " + 
                $"\n\rSubject. BootstrapContext: {subj.BootstrapContext}. Label: {subj.Label}. Identity and Claims subject are the same: {User.Identity == claim.Subject}");
        }

        [Authorize(Roles = "admin")]
        public IActionResult TestRole()
        {
            return Content($"Role (from claims): {User.FindFirst(x => x.Type == ClaimTypes.Role)}");
        }

        [Authorize("test")]
        public IActionResult TestPolicy()
        {
            return Content($"Policy test - OK");
        }

        //[Authorize("age")]
        [MinimumAgeAuthorize(20)]
        public IActionResult TestAge()
        {
            return Content($"Age test - OK");
        }

        [Authorize]
        public IActionResult Claims()
        {
            return View();
        }
    }
}
