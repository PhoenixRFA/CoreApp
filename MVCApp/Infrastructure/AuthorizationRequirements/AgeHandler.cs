using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCApp.Infrastructure.AuthorizationRequirements
{
    public class AgeHandler : AuthorizationHandler<AgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AgeRequirement requirement)
        {
            Claim claim = context.User.FindFirst(ClaimTypes.DateOfBirth);

            if(claim == null)
            {
                //garanty fail if other requirements successed
                context.Fail();
                return Task.CompletedTask;
            }

            if(int.TryParse(claim.Value, out int year))
            {
                int age = DateTime.Now.Year - year;

                if(age >= requirement.MinAge)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
            
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
