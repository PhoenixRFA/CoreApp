namespace IdentitySandboxApp.Models.Identity
{
    public class LoginWithRecoveryCodeModel
    {
        public string ReturnUrl { get; set; }
        public string RecoveryCode { get; set; }
    }
}
