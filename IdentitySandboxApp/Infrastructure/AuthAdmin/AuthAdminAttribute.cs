using Microsoft.AspNetCore.Authorization;

namespace IdentitySandboxApp.Infrastructure.AuthAdmin
{
    public class AuthAdminAttribute : AuthorizeAttribute
    {
        public AuthAdminAttribute()
        {
            Roles = "admin";
        }
    }
}
