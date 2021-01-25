using Microsoft.AspNetCore.Identity;

namespace IdentitySandboxApp.Models.Identity
{
    public class Role : IdentityRole<long>
    {
        public Role(string name) : base(name) { }
    }
}
