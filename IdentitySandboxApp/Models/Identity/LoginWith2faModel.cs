namespace IdentitySandboxApp.Models.Identity
{
    public class LoginWith2faModel
    {
        public bool RememberMachine { get; set; }
        public bool RememberMe { get; set; }
        public string TwoFactorCode { get; set; }
        public string ReturnUrl { get; set; }
    }
}
