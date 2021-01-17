using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

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

        #region Registration +

        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var model = new RegisterModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            bool isEmailEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Введите email");
                isEmailEmpty = true;
            }

            bool isUsernameEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Введите логин");
                isUsernameEmpty = true;
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
            
            bool isDateEmpty = false;
            if (string.IsNullOrWhiteSpace(model.DateOfBirth))
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Введите дату рождения");
                isDateEmpty = true;
            }
            ModelState.AddModelError("", "Testtesttest");
            if(isEmailEmpty || isUsernameEmpty || isPassEmpty || isPassConfirmEmpty || isDateEmpty)
            {
                return View(model);
            }

            bool isPasswordsNotSame = false;
            if(model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли должны совпадать");
                isPasswordsNotSame = true;
            }
            
            bool isEmailExists = false;
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Данный email уже зарегистрирован в системе. Попробуйте сбросить пароль");
                isEmailExists = true;
            }

            bool isUsernameExists = false;
            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Данный логин уже занят");
                isUsernameExists = true;
            }

            bool isDateInvalid = false;
            if (!DateTime.TryParse(model.DateOfBirth, out DateTime dateOfBirth))
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Не распознан формат даты");
                isDateInvalid = true;
            }
            
            if (isPasswordsNotSame || isEmailExists || isUsernameExists || isDateInvalid)
            {
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                UserName = model.Username,
                DateOfBirth = dateOfBirth,
                DateOfRegistration = DateTime.Now
            };

            IdentityResult res = await _userManager.CreateAsync(user, model.Password);
            if (res.Succeeded)
            {
                _logger.LogInformation("New user {user} created", user.UserName);

                string userId = await _userManager.GetUserIdAsync(user);
                    
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Подтверждение email", $"Для сброса пароля перейдите по <a href=\"{callbackUrl}\">ссылке</a>");// или введите код: <b>{code}</b>");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToAction("RegisterConfirmation", new { email = model.Email, returnUrl = model.ReturnUrl });
                }
                
                await _signInManager.SignInAsync(user, false);
                return LocalRedirect(model.ReturnUrl ?? "~/");
            }

            foreach (IdentityError error in res.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public IActionResult RegisterConfirmation()
        {
            ViewData["Title"] = "Подтверждение регистрации";
            ViewData["Message"] = "Проверьте ваш email для завершения регистрации";

            return View("Common");
        }

        #endregion

        #region Login/Logout +

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewBag.ReturnUrl = returnUrl;

            return View(new LoginModel());
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

            SignInResult res = await _signInManager.PasswordSignInAsync(model.Login, model.Password, model.RememberMe, true);

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
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("~/");
        }

        #endregion

        #region 2FA

        [HttpGet]
        public async Task<IActionResult> LoginWith2Fa(string returnUrl = null, bool rememberMe = false)
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
        public async Task<IActionResult> LoginWith2Fa(LoginWith2faModel model)
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

            SignInResult res = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.TwoFactorCode, model.RememberMe, model.RememberMachine);

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

            SignInResult res = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.RecoveryCode);

            if (res.Succeeded)
            {
                _logger.LogInformation("{user} is logged in with recovery code", user.UserName);
                return LocalRedirect(model.ReturnUrl ?? "~/");
            }

            if (res.IsLockedOut)
            {
                _logger.LogWarning("{user} is locked out", user.UserName);
                return RedirectToAction("Lockout");
            }

            _logger.LogWarning("{user} is entered wrong recovery code", user.UserName);
            ModelState.AddModelError(string.Empty, "Не верный код восстановления");
            return View();
        }


        #endregion

        #region External login

        [HttpGet]
        public IActionResult ExternalLogin() => RedirectToAction("Login");

        [HttpPost] //Request a redirect to the external login provider
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            string redirectUrl = Url.Action("ExternalLoginCallback", new { returnUrl });
            
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            
            return new ChallengeResult(provider, properties);
        }
        
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ViewData["RemoteError"] = remoteError;
                return RedirectToAction("Login", new { ReturnUrl = returnUrl});
            }

            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Error loading external login information.");
                ViewData["RemoteError"] = "Ошибка получения информации о внешнем провайдере авторизации";
                return RedirectToAction("Login", new { ReturnUrl = returnUrl});
            }

            //Sign in the user with this external login provider if the user already has a login.
            SignInResult res = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            if (res.Succeeded)
            {
                _logger.LogInformation("{user} is logged in with {provider} provider", info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl ?? "~/");
            }

            if (res.IsLockedOut)
            {
                _logger.LogWarning("{user} is locked out", info.Principal.Identity?.Name, info.LoginProvider);
                return RedirectToAction("Lockout");
            }

            var model = new ExternalLoginModel
            {
                ReturnUrl = returnUrl,
                ProviderName = info.ProviderDisplayName
            };

            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                model.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                model.Username = model.Email;
            }

            return View("ExternalLogin", model);
        }

        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel model)
        {
            bool isEmailEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Введите email");
                isEmailEmpty = true;
            }

            bool isUsernameEmpty = false;
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Введите логин");
                isUsernameEmpty = true;
            }

            bool isDateEmpty = false;
            if (string.IsNullOrWhiteSpace(model.DateOfBirth))
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Введите дату рождения");
                isDateEmpty = true;
            }

            if (isEmailEmpty || isUsernameEmpty || isDateEmpty)
            {
                return View("ExternalLogin");
            }

            bool isEmailExists = false;
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Данный email уже зарегистрирован в системе. Попробуйте сбросить пароль");
                isEmailExists = true;
            }

            bool isUsernameExists = false;
            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError(nameof(model.Username), "Данный логин уже занят");
                isUsernameExists = true;
            }

            if (isEmailExists || isUsernameExists)
            {
                return View("ExternalLogin");
            }

            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Error loading external login information.");
                ViewData["RemoteError"] = "Ошибка получения информации о внешнем провайдере авторизации";
                return RedirectToAction("Login", new { model.ReturnUrl });
            }

            var user = new User {
                UserName = model.Email,
                Email = model.Email
            };

            IEnumerable<string> errors = null;
            string errorMsg = null;
            IdentityResult userCreateResult = await _userManager.CreateAsync(user);
            if (userCreateResult.Succeeded)
            {
                IdentityResult addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    _logger.LogInformation("User {user} created an account using {provider} provider", user.UserName, info.LoginProvider);

                    string userId = await _userManager.GetUserIdAsync(user);
                    
                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Подтверждение email", $"Для сброса пароля перейдите по <a href=\"{callbackUrl}\">ссылке</a>");// или введите код: <b>{code}</b>");

                    //If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToAction("RegisterConfirmation", new { model.Email });
                    }

                    await _signInManager.SignInAsync(user, false, info.LoginProvider);

                    return LocalRedirect(model.ReturnUrl ?? "~/");
                }

                if (addLoginResult.Errors.Any())
                {
                    errors = addLoginResult.Errors.Select(x => $"{x.Code}: {x.Description}");
                    errorMsg = string.Join(", ", errors);
                    _logger.LogWarning("Ext. Login Conf. add login error: {error}", errorMsg);
                }
            }

            if (userCreateResult.Errors.Any())
            {
                errors = userCreateResult.Errors.Select(x => $"{x.Code}: {x.Description}");
                errorMsg = string.Join(", ", errors);
                _logger.LogWarning("Ext. Login Conf. user create error: {error}", errorMsg);
            }

            return RedirectToAction("Login");
        }

        #endregion

        #region Forgot/Reset password +

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
            ViewData["Title"] = "Восстановление пароля";
            ViewData["Message"] = "Проверьте ваш email для сброса пароля";
            return View("Common");
        }

        [HttpGet]
        public IActionResult ResetPassword(string code)
        {
            if (code == null)
            {
                return BadRequest($"Field {nameof(code)} is empty");
            }

            ViewBag.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            return View(new ResetPasswordModel{Code = code});
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

            if(model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли должны совпадать");
                return View();
            }

            User user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            string code = HttpUtility.HtmlDecode(model.Code);
            IdentityResult res = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (res.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (IdentityError error in res.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        public IActionResult ResetPasswordConfirmation()
        {
            ViewData["Title"] = "Подтверждение email";
            ViewData["Message"] = $"Пароль был сброшен. <a href=\"{Url.Action("Login")}\">Нажмите что бы войти</a>";

            return View("Common");
        }

        #endregion

        #region Confirm Email +

        public async Task<IActionResult> ConfirmEmail(long userId, string code)
        {
            if (userId <= 0 || code == null)
            {
                return LocalRedirect("~/");
            }

            User user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return LocalRedirect("~/");
            }

            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult res = await _userManager.ConfirmEmailAsync(user, token);

            ViewData["Message"] = res.Succeeded ? "Спасибо за подтверждение email" : "Не удалось подтвердить email";
            
            if (!res.Succeeded)
            {
                _logger.LogWarning("Cannot confirm {user} email", user.UserName);
            }

            ViewData["Title"] = "Подтверждение email";
            return View("Common");
        }
        
        
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Введите email");
                return View();
            }

            User user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewData["Message"] = "Подтверждение отправлено на ваш email";
                return View();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

            await _emailSender.SendEmailAsync(email, "Подтверждение email", $"Для сброса пароля перейдите по <a href=\"{callbackUrl}\">ссылке</a>");// или введите код: <b>{code}</b>");

            ViewData["Message"] = "Подтверждение отправлено на ваш email";
            return View();
        }

        #endregion

        /*Identity error codes:
            DefaultError
            ConcurrencyFailure
            PasswordMismatch
            InvalidToken
            LoginAlreadyAssociated
            InvalidUserName
            InvalidEmail
            DuplicateUserName
            DuplicateEmail
            InvalidRoleName
            DuplicateRoleName
            UserAlreadyHasPassword
            UserLockoutNotEnabled
            UserAlreadyInRole
            UserNotInRole
            PasswordTooShort
            PasswordRequiresNonAlphanumeric 
            PasswordRequiresDigit
            PasswordRequiresLower
            PasswordRequiresUpper
        */
    }
}
