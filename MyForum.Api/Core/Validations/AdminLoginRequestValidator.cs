using FluentValidation;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Validations
{
    public class AdminLoginRequestValidator : AbstractValidator<AdminLoginRequest>
    {
        public AdminLoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Имя пользователя обязательно для входа.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Пароль обязателен для входа.");
        }
    }
}