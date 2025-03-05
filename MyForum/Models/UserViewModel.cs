using System.ComponentModel.DataAnnotations;

namespace MyForum.Models
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Введите имя.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Количество символов должно быть не меньше 4 и не больше 50.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Введите почту")]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Некорректный адресс почты.")]
        [MaxLength(100, ErrorMessage = "Длина не должна превышать больше 100 символов.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Диапазон длины пароля должен входить от 6 до 255 символов.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Пароли не совпадают.")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string PasswordConfirm { get; set; } = null!;
    }
}
