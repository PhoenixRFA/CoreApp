namespace IdentitySandboxApp.Models.Identity
{
    public class TwoFactorAuthenticationModel
    {
        public bool HasAuthenticator { get; set; }
        public bool Is2faEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
        public int RecoveryCodesLeft { get; set; }
    }
}
