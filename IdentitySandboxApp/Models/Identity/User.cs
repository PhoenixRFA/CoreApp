using System;
using Microsoft.AspNetCore.Identity;

namespace IdentitySandboxApp.Models.Identity
{
    public class User : IdentityUser<long>
    {
        public DateTime DateOfRegistration { get; set; }
        [PersonalData]
        public DateTime DateOfBirth { get; set; }
    }
}
