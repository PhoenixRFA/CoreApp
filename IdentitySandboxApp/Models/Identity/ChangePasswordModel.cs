using System.ComponentModel.DataAnnotations;

namespace IdentitySandboxApp.Models.Identity
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Введите старый пароль")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "Введите новый пароль")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Повторите новый пароль"), Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
        public string Message { get; set; }
    }
}
