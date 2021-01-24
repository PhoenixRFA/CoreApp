using System.ComponentModel.DataAnnotations;

namespace IdentitySandboxApp.Models.Users
{
    public class CreateUserModel
    {
        [Required(ErrorMessage = "Введите логин"), MinLength(4, ErrorMessage = "Минимальная длина логина - 4 символа")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Введите email"), EmailAddress(ErrorMessage = "Ошибка в email'е")]
        public string Email { get; set; }
        [Phone(ErrorMessage = "Ошибка в номере")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Укажите дату рождения")]
        public string DateOfBirth { get; set; }
        [Required(ErrorMessage = "Введите пароль"), MinLength(6, ErrorMessage = "Минимальная длина пароля - 6 символов")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Введите подтверждение пароля"), Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
