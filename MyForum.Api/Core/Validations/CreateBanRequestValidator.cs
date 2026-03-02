using FluentValidation;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Validations
{
    public class CreateBanRequestValidator : AbstractValidator<CreateBanRequest>
    {
        public CreateBanRequestValidator()
        {
            RuleFor(x => x.IpHash)
                .NotNull()
                .WithMessage("Хэш IP-адреса не может быть пустым.")
                .NotEmpty()
                .WithMessage("Хэш IP-адреса обязателен.")
                .Length(44)
                .WithMessage("Хэш IP-адреса должен быть длиной 44 символа (формат SHA256 Base64).")
                .Matches("^[A-Za-z0-9+/]{43}={0,2}$")
                .WithMessage("Хэш IP-адреса должен быть в формате Base64.");

            RuleFor(x => x.Reason)
                .NotNull()
                .WithMessage("Причина бана не может быть пустой.")
                .NotEmpty()
                .WithMessage("Причина бана обязательна для ввода.")
                .MinimumLength(5)
                .WithMessage("Причина бана должна содержать не менее 5 символов.")
                .MaximumLength(1000)
                .WithMessage("Причина бана не должна превышать 1000 символов.");

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.UtcNow)
                .When(x => x.ExpiresAt.HasValue)
                .WithMessage("Дата окончания бана должна быть в будущем.")
                .LessThan(DateTime.UtcNow.AddYears(10))
                .When(x => x.ExpiresAt.HasValue)
                .WithMessage("Дата окончания бана не может быть слишком отдалённой.");
        }
    }
}