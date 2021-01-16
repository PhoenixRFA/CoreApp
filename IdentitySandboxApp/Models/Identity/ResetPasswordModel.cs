namespace IdentitySandboxApp.Models.Identity
{
    public class ResetPasswordModel
    {
        public string Code { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
