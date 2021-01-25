using System.ComponentModel.DataAnnotations;

namespace IdentitySandboxApp.Models.Users
{
    public class EditUserModel
    {
        [Required(ErrorMessage = "Введите логин"), MinLength(4, ErrorMessage = "Минимальная длина логина - 4 символа")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Введите email"), EmailAddress(ErrorMessage = "Ошибка в email'е")]
        public string Email { get; set; }
        [Phone(ErrorMessage = "Ошибка в номере")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Укажите дату рождения")]
        public string DateOfBirth { get; set; }

        public long Id { get; set; }
        public bool CanDelete { get; set; }
    }
}
