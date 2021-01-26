using System.Security.Claims;
using System.Threading.Tasks;
using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentitySandboxApp.Infrastructure.ClaimsFactory
{
    public class TestClaimsFactory : UserClaimsPrincipalFactory<User>
    {
        public TestClaimsFactory(UserManager<User> userManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);

            identity.AddClaim(new Claim("Test", "foobar"));

            return identity;
        }
    }
}
