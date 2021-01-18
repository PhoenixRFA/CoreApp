namespace IdentitySandboxApp.Models.Identity
{
    public class EmailManageModel
    {
        public bool IsEmailConfirmed { get; set; }
        public string Email { get; set; }
        public string NewEmail { get; set; }
        public string Message { get; set; }
    }
}
