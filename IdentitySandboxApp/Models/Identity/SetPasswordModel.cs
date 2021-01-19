namespace IdentitySandboxApp.Models.Identity
{
    public class SetPasswordModel
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string Message { get; set; }
    }
}
