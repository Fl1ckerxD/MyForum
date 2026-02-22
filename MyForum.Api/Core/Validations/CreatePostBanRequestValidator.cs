using FluentValidation;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Validations
{
    public class CreatePostBanRequestValidator : AbstractValidator<CreatePostBanRequest>
    {
        public CreatePostBanRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotNull()
                .WithMessage("Причина бана не может быть пустой.")
                .NotEmpty()
                .WithMessage("Причина бана обязательна для ввода.")
                .MinimumLength(5)
                .WithMessage("Причина бана должна содержать не менее 5 символов.")
                .MaximumLength(1000)
                .WithMessage("Причина бана не должна превышать 1000 символов.");

            RuleFor(x => x.BoardId)
                .GreaterThanOrEqualTo(1)
                .When(x => x.BoardId.HasValue)
                .WithMessage("Идентификатор доски должен быть положительным числом.");

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