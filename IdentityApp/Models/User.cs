using Microsoft.AspNetCore.Identity;
using System;

namespace IdentityApp.Models
{
    public class User : IdentityUser<long>
    {
        public DateTime? DateOfBirth { get; set; }
    }
}
