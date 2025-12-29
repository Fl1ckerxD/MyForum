using FluentValidation;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Validations
{
    public class CreateThreadRequestValidator : AbstractValidator<CreateThreadRequest>
    {
        public CreateThreadRequestValidator(IConfiguration configuration)
        {
            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Тема треда обязательна")
                .MaximumLength(100).WithMessage("Тема не более 100 символов");

            RuleFor(x => x.BoardShortName)
                .NotEmpty().WithMessage("Доска обязательна")
                .Matches("^[a-z]+$").WithMessage("Некорректное имя доски");
            
            RuleFor(x => x.OriginalPost)
                .SetValidator(new CreatePostRequestValidator(configuration));
        }
    }
}