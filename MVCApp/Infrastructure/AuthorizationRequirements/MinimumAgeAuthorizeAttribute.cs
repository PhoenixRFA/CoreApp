using Microsoft.AspNetCore.Authorization;

namespace MVCApp.Infrastructure.AuthorizationRequirements
{
    public class MinimumAgeAuthorizeAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "Age";
        
        public MinimumAgeAuthorizeAttribute(int minAge)
        {
            MinAge = minAge;

            Policy = "age";
        }

        public int MinAge
        {
            get
            {
                if (int.TryParse(Policy.Substring(POLICY_PREFIX.Length), out int age))
                {
                    return age;
                }
                return 0;
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value}";
            }
        }
    }
}
