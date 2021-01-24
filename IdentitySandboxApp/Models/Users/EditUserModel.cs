using IdentitySandboxApp.Models.Identity;

namespace IdentitySandboxApp.Models.Users
{
    public class EditUserModel
    {
        public User User { get; set; }
        public bool CanDelete { get; set; }
    }
}
