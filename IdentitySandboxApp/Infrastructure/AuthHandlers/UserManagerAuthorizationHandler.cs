using System.Threading.Tasks;
using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace IdentitySandboxApp.Infrastructure.AuthHandlers
{
    public class UserManagerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, User>
    {
        private readonly UserManager<User> _userManger;
        public UserManagerAuthorizationHandler(UserManager<User> userManager)
        {
            _userManger = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, User resource)
        {
            if(resource == null)
            {
                context.Fail();
                return;
            }

            User user = await _userManger.GetUserAsync(context.User);
            if(user == null)
            {
                context.Fail();
                return;
            }

            if (resource.UserName == "admin")
            {
                context.Fail();
                return;
            }

            if (await _userManger.IsInRoleAsync(user, "admin"))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
