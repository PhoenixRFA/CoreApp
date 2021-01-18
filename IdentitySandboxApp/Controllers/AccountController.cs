﻿using IdentitySandboxApp.Models.Identity;
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
using Microsoft.AspNetCore.Authentication;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using System.Text.Json;
using System.Text.Encodings.Web;

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

        public async Task<IActionResult> Index()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Unable to load user");
            }

            await LoadAsync(user);
            return View(model);
        }

        public async Task<IActionResult> Submit()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return View("Index");
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return View("Index", model);
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }


        public async Task<IActionResult> PersonalData()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction($"Unable to load user");
            }

            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> ResetAuthenticator()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetAuthenticator()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);
            
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToPage("EnableAuthenticator");
        }

        [HttpGet]
        public async Task<IActionResult> ResetAuthenticator()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToPage("./ChangePassword");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetAuthenticator()
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            IdentityResult addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your password has been set.";

            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> ShowRecoveryCodes()
        {
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToAction("TwoFactorAuthentication");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return View(model);
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
                //TODO Extract method and cache GetExternalAuthenticationSchemesAsync result
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            //TODO Extract validation
            #region Validation

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

            if (isEmailEmpty || isUsernameEmpty || isPassEmpty || isPassConfirmEmpty || isDateEmpty)
            {
                return View(model);
            }

            bool isPasswordsNotSame = false;
            if (model.Password != model.ConfirmPassword)
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

            #endregion

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

                //TODO Extract method
                #region Generate confirm ermail token

                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Подтверждение email", $"Для сброса пароля перейдите по <a href=\"{callbackUrl}\">ссылке</a>");// или введите код: <b>{code}</b>");

                #endregion

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
            //TODO See early
            ViewBag.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewBag.ReturnUrl = returnUrl;

            return View(new LoginModel());
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            //bool loginIsEmpty = false;
            //if (string.IsNullOrWhiteSpace(model.Login))
            //{
            //    ModelState.AddModelError(nameof(model.Login), "Введите логин");
            //    loginIsEmpty = true;
            //}

            //bool passIsEmpty = false;
            //if (string.IsNullOrWhiteSpace(model.Password))
            //{
            //    ModelState.AddModelError(nameof(model.Password), "Введите пароль");
            //    passIsEmpty = true;
            //}

            //if(loginIsEmpty || passIsEmpty)
            //{
            //    return View();
            //}

            //TODO Use model validation via annotations
            if (!ModelState.IsValid)
            {
                return View(model);
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

            //TODO See earlier
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

            if (user == null)
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
            if (user == null)
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
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Error loading external login information.");
                ViewData["RemoteError"] = "Ошибка получения информации о внешнем провайдере авторизации";
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
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
            //TODO Use model & model validation
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Введите пароль");
                return View();
            }

            User user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            //TODO Extract method
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            string encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
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

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            return View(new ResetPasswordModel { Code = code });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            //TODO Use data annotations

            #region Validation

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

            if (isEmailEmpty || isPassEmpty || isPassConfirmEmpty)
            {
                return View();
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли должны совпадать");
                return View();
            }

            #endregion

            User user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            //Extract method for decode
            string code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
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
            //TODO Use model & validation
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

            //TODO Extract method
            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId, code }, Request.Scheme);

            await _emailSender.SendEmailAsync(email, "Подтверждение email", $"Для сброса пароля перейдите по <a href=\"{callbackUrl}\">ссылке</a>");// или введите код: <b>{code}</b>");

            ViewData["Message"] = "Подтверждение отправлено на ваш email";
            return View();
        }

        #endregion


        #region

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            //TODO Remove or extract
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction("SetPassword");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            IdentityResult res = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!res.Succeeded)
            {
                foreach (var error in res.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            //?
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User {user} changed password successfully", user.UserName);
            model.Message = "Your password has been changed.";

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeletePersonalData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var model = new DeletePersonalDataModel {
                RequirePassword = await _userManager.HasPasswordAsync(user)
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> DeletePersonalData(DeletePersonalDataModel model)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            model.RequirePassword = await _userManager.HasPasswordAsync(user);

            if (model.RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Не верный пароль");
                    return View();
                }
            }

            IdentityResult result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                string errors = string.Join(" \n\r", result.Errors.Select(x => $"{x.Code}: {x.Description}"));
                _logger.LogWarning("Error on {user} user delete.\r\nErrors: {errors}", user.UserName, errors);
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User {user} deleted themselves", user.UserName);

            return Redirect("~/");
        }

        [HttpGet]
        public async Task<IActionResult> Disable2fa()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return RedirectToAction("Index");
            }

            return View();
        }
        [HttpPost]
        [ActionName("Disable2fa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoDisable2fa()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            IdentityResult result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                string errors = string.Join(" \n\r", result.Errors.Select(x => $"{x.Code}: {x.Description}"));
                _logger.LogWarning("Error on {user} user disabling 2fa.\r\nErrors: {errors}", user.UserName, errors);
            }

            _logger.LogInformation("User {user} has disabled 2fa", _userManager.GetUserId(User));
            
            ViewBag.Message = "Двухфакторная авторизация была отключена";
            return RedirectToPage("TwoFactorAuthentication");
        }

        public async Task<IActionResult> DownloadPersonalData()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            _logger.LogInformation("User {user} asked for their personal data", user);

            //Only include personal data for download
            var personalData = new Dictionary<string, string>();
            //TODO Reflexy?
            var personalDataProps = typeof(User).GetProperties().Where(x => Attribute.IsDefined(x, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            //TODO ?
            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> Email()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            string email = await _userManager.GetEmailAsync(user);

            var model = new EmailManageModel {
                Email = email,
                NewEmail = email,
                IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user)
            };

            return View(model);
        }

        public async Task<IActionResult> ChangeEmail(EmailManageModel model)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            string email = await _userManager.GetEmailAsync(user);
            model.Email = email;
            model.IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!ModelState.IsValid)
            {
                return View("Email", model);
            }

            if (model.NewEmail != email)
            {
                string userId = await _userManager.GetUserIdAsync(user);
                string code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string callbackUrl = Url.Action("ConfirmEmailChange", "Account", new { userId, email = model.NewEmail, code }, Request.Scheme);
                await _emailSender.SendEmailAsync(model.NewEmail, "Подтверждение Email'а", $"Для подтверждения email'а - перейдите по ссылке: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>подтвердить</a>");

                model.Message = "Подтверждение было отправлно на email";
                return View("Email", model);
            }

            model.Message = "Email не был изменен";
            return View("Email", model);
        }

        public async Task<IActionResult> SendVerificationEmail(EmailManageModel model)
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            string email = await _userManager.GetEmailAsync(user);
            model.Email = email;
            model.IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!ModelState.IsValid)
            {
                return View("Email", model);
            }

            string userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userId, code = code }, Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Подтверждение Email'а", $"Для подтверждения email'а - перейдите по ссылке: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>подтвердить</a>");

            model.Message = "Подтверждение было отправлно на email";

            return View("Email", model);
        }

        //TODO fix methods
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            await LoadSharedKeyAndQrCodeUriAsync(user);

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return View(model);
            }

            // Strip spaces and hypens
            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("ShowRecoveryCodes");
            }
            else
            {
                return RedirectToAction("TwoFactorAuthentication");
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(User user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(AuthenticatorUriFormat, _urlEncoder.Encode("IdentitySandboxApp"), _urlEncoder.Encode(email), unformattedKey);
        }


        //TODO complete methods
        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index"); ;
            }

            CurrentLogins = await _userManager.GetLoginsAsync(user);
            OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            ShowRemoveButton = user.PasswordHash != null || CurrentLogins.Count > 1;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginAsync(string loginProvider, string providerKey)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not removed.";
                return View("ExternalLogins");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return View("ExternalLogins");
        }
        [HttpPost]
        public async Task<IActionResult> LinkLoginAsync(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }
        [HttpGet]
        public async Task<IActionResult> LinkLoginCallbackAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID 'user.Id'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id.ToString());
            if (info == null)
            {
                throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not added. External logins can only be associated with one account.";
                return RedirectToPage();
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToPage();
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            User user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!isTwoFactorEnabled)
            {
                return RedirectToAction("Index");
            }

            IEnumerable<string> recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation("User {UserId} has generated new 2FA recovery codes", user.UserName);
            StatusMessage = "You have generated new recovery codes.";
            return RedirectToPage("ShowRecoveryCodes");
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
