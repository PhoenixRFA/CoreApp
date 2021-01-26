using IdentitySandboxApp.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

namespace IdentitySandboxApp.Infrastructure.AuthHandlers
{
    public class TestAuthHandler : AuthorizationHandler<OperationAuthorizationRequirement, CreateUserModel>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, CreateUserModel resource)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
