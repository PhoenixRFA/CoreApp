using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace IdentitySandboxApp.Models.Identity
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string ReturnUrl { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string DateOfBirth { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
