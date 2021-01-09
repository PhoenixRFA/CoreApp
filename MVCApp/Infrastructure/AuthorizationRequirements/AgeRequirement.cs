using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCApp.Infrastructure.AuthorizationRequirements
{
    public class AgeRequirement : IAuthorizationRequirement
    {
        protected internal int MinAge { get; set; }

        public AgeRequirement(int minAge)
        {
            MinAge = minAge;
        }
    }
}
