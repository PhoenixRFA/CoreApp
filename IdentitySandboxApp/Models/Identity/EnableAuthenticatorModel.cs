using System.ComponentModel.DataAnnotations;

namespace IdentitySandboxApp.Models.Identity
{
    public class EnableAuthenticatorModel
    {
        [Required]
        public string Code { get; set; }
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
    }
}
