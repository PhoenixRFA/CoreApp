using System.ComponentModel.DataAnnotations;

namespace IdentitySandboxApp.Models.Users
{
    public class ChangeUserPasswordModel
    {
        [Required(ErrorMessage = "Введите пароль"), MinLength(6, ErrorMessage = "Минимальная длина пароля - 6 символов")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Введите подтверждение пароля"), Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
