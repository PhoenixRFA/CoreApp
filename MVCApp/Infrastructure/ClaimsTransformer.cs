using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCApp.Infrastructure
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // This will run every time Authenticate is called so its better to create a new Principal
            var transformed = new ClaimsPrincipal();
            
            transformed.AddIdentities(principal.Identities);
            transformed.AddIdentity(new ClaimsIdentity(new Claim[]
            {
                new Claim("Transformed", DateTime.Now.ToString())
            }));
            return Task.FromResult(transformed);
        }
    }
}
