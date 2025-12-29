using FluentValidation;
using MyForum.Api.Core.DTOs.Requests;

namespace MyForum.Api.Core.Validations
{
    public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
    {
        private readonly IConfiguration _configuration;
        public CreatePostRequestValidator(IConfiguration configuration)
        {
            _configuration = configuration;
            int maxFilesPerPost = _configuration.GetValue<int>("MinIO:MaxFilesPerPost");
            int maxFileSize = _configuration.GetValue<int>("MinIO:MaxFileSize");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Сообщение не может быть пустым")
                .MaximumLength(2000).WithMessage("Сообщение не более 2000 символов");

            RuleFor(x => x.AuthorName)
                .MaximumLength(50).WithMessage("Имя не более 50 символов")
                .Matches("^[a-zA-Zа-яА-Я0-9 ]*$").WithMessage("Недопустимые символы в имени");

            RuleFor(x => x.PostPassword)
                .MaximumLength(20).WithMessage("Пароль не более 20 символов")
                .When(x => !string.IsNullOrEmpty(x.PostPassword));

            RuleFor(x => x.Files)
                .Must(files => files == null || files.Count <= maxFilesPerPost)
                .WithMessage($"Максимум {maxFilesPerPost} файла");

            RuleForEach(x => x.Files)
                .ChildRules(file =>
                {
                    file.RuleFor(f => f.Length)
                        .LessThanOrEqualTo(maxFileSize)
                        .WithMessage("Файл слишком большой");

                    file.RuleFor(f => f.FileName)
                        .Must(BeSupportedFileType)
                        .WithMessage("Неподдерживаемый тип файла");
                });
        }

        private bool BeSupportedFileType(string fileName)
        {
            var allowedExtensions = _configuration.GetSection("MinIO:AllowedExtensions").Get<string[]>();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}