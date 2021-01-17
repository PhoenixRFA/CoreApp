namespace IdentitySandboxApp.Models.Identity
{
    public class ExternalLoginModel
    {
        public string Email { get; set; }
        public string ReturnUrl { get; set; }
        public string ProviderName { get; set; }
    }
}
