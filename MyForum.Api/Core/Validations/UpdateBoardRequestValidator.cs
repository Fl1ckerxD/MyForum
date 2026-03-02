using FluentValidation;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Validations
{
    public class UpdateBoardRequestValidator : AbstractValidator<UpdateBoardRequest>
    {
        public UpdateBoardRequestValidator()
        {
            RuleFor(x => x.ShortName)
                .NotEmpty().WithMessage("Короткое имя не может быть пустым")
                .MaximumLength(10).WithMessage("Короткое имя не более 10 символов")
                .Matches("^[a-zA-Z]+$").WithMessage("Короткое имя может содержать только буквы латинского алфавита");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название не может быть пустым")
                .MaximumLength(100).WithMessage("Название не более 100 символов");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Описание не более 500 символов");
        }
    }
}