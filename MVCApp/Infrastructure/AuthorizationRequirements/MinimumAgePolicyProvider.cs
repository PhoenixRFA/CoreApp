using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace MVCApp.Infrastructure.AuthorizationRequirements
{
    public class MinimumAgePolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return Task.FromResult(new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return Task.FromResult<AuthorizationPolicy>(null);
        }

        const string POLICY_PREFIX = "Age";
        private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

        public MinimumAgePolicyProvider(IOptions<AuthorizationOptions> options)
        {
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith("age", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(policyName.Substring(POLICY_PREFIX.Length), out var age))
            {
                var policy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme);
                policy.AddRequirements(new AgeRequirement(age));
                return Task.FromResult(policy.Build());
            }

            return BackupPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
