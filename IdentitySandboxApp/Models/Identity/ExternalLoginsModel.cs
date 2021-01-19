using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace IdentitySandboxApp.Models.Identity
{
    public class ExternalLoginsModel
    {
        public List<UserLoginInfo> CurrentLogins { get; set; }
        public List<AuthenticationScheme> OtherLogins { get; set; }
        public bool ShowRemoveButton { get; set; }
    }
}
