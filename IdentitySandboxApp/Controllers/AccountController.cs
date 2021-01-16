using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IdentitySandboxApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Lockout()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            bool loginIsEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Login))
            {
                ModelState.AddModelError(nameof(model.Login), "Введите логин");
                loginIsEmpty = true;
            }

            bool passIsEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Введите пароль");
                passIsEmpty = true;
            }

            if(loginIsEmpty || passIsEmpty)
            {
                return View();
            }

            Microsoft.AspNetCore.Identity.SignInResult res = await _signInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, true);

            if (res.Succeeded)
            {
                _logger.LogInformation("{user} is logged in", model.Login);
                return LocalRedirect(model.ReturnUrl ?? "~/");
            }

            if (res.RequiresTwoFactor)
            {
                return RedirectToAction("", new { returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
            }

            if (res.IsLockedOut)
            {
                _logger.LogWarning("{user} is locked out", model.Login);
                return RedirectToAction("Lockout");
            }

            ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewBag.ReturnUrl = model.ReturnUrl;

            ModelState.AddModelError(string.Empty, "Не верный логин и/или пароль");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(string returnUrl = null, bool rememberMe = false)
        {
            User user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            
            if(user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.RemeberMe = rememberMe;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faModel model)
        {
            User user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if(user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user");
            }

            if (string.IsNullOrWhiteSpace(model.TwoFactorCode))
            {
                ModelState.AddModelError(nameof(model.TwoFactorCode), "Введите код");
                return View();
            }

            Microsoft.AspNetCore.Identity.SignInResult res = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.TwoFactorCode, model.RememberMe, model.RememberMachine);

            if (res.Succeeded)
            {
                _logger.LogInformation("{user} is logged in with 2fa", user.UserName);
                return LocalRedirect(model.ReturnUrl ?? "~/");
            }

            if (res.IsLockedOut)
            {
                _logger.LogWarning("{user} is locked out", user.UserName);
                return RedirectToAction("Lockout");
            }

            _logger.LogWarning("{user} entered invalid 2fa code", user.UserName);
            ModelState.AddModelError(string.Empty, "Не верный код авторизации");
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("~/");
        }

        [HttpGet]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            User user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeModel model)
        {
            User user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user");
            }

            if (string.IsNullOrWhiteSpace(model.RecoveryCode))
            {
                ModelState.AddModelError(nameof(model.RecoveryCode), "Введите код восстановления");
                return View();
            }

            Microsoft.AspNetCore.Identity.SignInResult res = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.RecoveryCode);

            if (res.Succeeded)
            {
                _logger.LogInformation("{user} is logged in with recovery code", user.UserName);
                return LocalRedirect(model.ReturnUrl ?? "~/");
            }

            if (res.IsLockedOut)
            {
                _logger.LogWarning("{user} is locked out", user.UserName);
                return RedirectToAction("Locked");
            }

            _logger.LogWarning("{user} is entered wrong recovery code", user.UserName);
            ModelState.AddModelError(string.Empty, "Не верный код восстановления");
            return View();
        }
        
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Введите пароль");
                return View();
            }

            User user = await _userManager.FindByEmailAsync(email);
            if(user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            string encodedCode = HttpUtility.HtmlEncode(code);
            //string encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Action("ResetPassword", "Account", new { code = encodedCode }, Request.Scheme);

            await _emailSender.SendEmailAsync(email, "Сброс пароля", $"Для сброса пароля перейдите по <a href=\"{callbackUrl}\">ссылке</a>");// или введите код: <b>{code}</b>");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code)
        {
            if (code == null)
            {
                return BadRequest($"Field {nameof(code)} is empty");
            }

            ViewBag.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            bool isEmailEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Введите email");
                isEmailEmpty = true;
            }

            bool isPassEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Введите пароль");
                isPassEmpty = true;
            }

            bool isPassConfirmEmpty = false;
            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Введите пароль");
                isPassConfirmEmpty = true;
            }

            if(isEmailEmpty || isPassEmpty || isPassConfirmEmpty)
            {
                return View();
            }

            if(model.Password == model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли должны совпадать");
                return View();
            }

            User user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            IdentityResult res = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (res.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (IdentityError error in res.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

    }
}
