using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.Validations;

namespace MyForum.Api.Tests.Validations
{
    public class CreateThreadRequestValidatorTests
    {
        private readonly CreateThreadRequestValidator _validator;

        public CreateThreadRequestValidatorTests()
        {
            var inMemory = new Dictionary<string, string?>
            {
                ["FileUpload:MaxFilesPerPost"] = "4",
                ["FileUpload:MaxFileSize"] = (4 * 1024 * 1024).ToString(),
                ["FileUpload:AllowedExtensions:0"] = ".jpg",
                ["FileUpload:AllowedExtensions:1"] = ".png"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory)
                .Build();

            _validator = new CreateThreadRequestValidator(configuration);
        }

        [Fact]
        public void ShouldHaveErrorWhenSubjectIsEmpty()
        {
            // Arrange
            var model = new CreateThreadRequest(
                BoardId: 1,
                BoardShortName: "general",
                Subject: "",
                OriginalPost: new CreatePostRequest(
                    ThreadId: 0,
                    Content: "This is the first post in the thread.",
                    AuthorName: "TestUser",
                    Files: null
                )
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Subject)
                .WithErrorMessage("Тема треда обязательна");
        }

        [Fact]
        public void ShouldHaveErrorWhenSubjectTooLong()
        {
            // Arrange
            var longSubject = new string('a', 101);
            var model = new CreateThreadRequest(
                BoardId: 1,
                BoardShortName: "general",
                Subject: longSubject,
                OriginalPost: new CreatePostRequest(
                    ThreadId: 0,
                    Content: "This is the first post in the thread.",
                    AuthorName: "TestUser",
                    Files: null
                )
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Subject)
                .WithErrorMessage("Тема не более 100 символов");
        }

        [Fact]
        public void ShouldHaveErrorWhenBoardShortNameIsEmpty()
        {
            // Arrange
            var model = new CreateThreadRequest(
                BoardId: 1,
                BoardShortName: "",
                Subject: "Test Subject",
                OriginalPost: new CreatePostRequest(
                    ThreadId: 0,
                    Content: "This is the first post in the thread.",
                    AuthorName: "TestUser",
                    Files: null
                )
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BoardShortName)
                .WithErrorMessage("Доска обязательна");
        }

        [Fact]
        public void ShouldHaveErrorWhenBoardShortNameIsInvalid()
        {
            // Arrange
            var model = new CreateThreadRequest(
                BoardId: 1,
                BoardShortName: "General123",
                Subject: "Test Subject",
                OriginalPost: new CreatePostRequest(
                    ThreadId: 0,
                    Content: "This is the first post in the thread.",
                    AuthorName: "TestUser",
                    Files: null
                )
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BoardShortName)
                .WithErrorMessage("Некорректное имя доски");
        }

        [Fact]
        public void ShouldHaveErrorWhenOriginalPostIsInvalid()
        {
            // Arrange
            var model = new CreateThreadRequest(
                BoardId: 1,
                BoardShortName: "general",
                Subject: "Test Subject",
                OriginalPost: new CreatePostRequest(
                    ThreadId: 0,
                    Content: "", // Invalid content
                    AuthorName: "TestUser",
                    Files: null
                )
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor("OriginalPost.Content")
                .WithErrorMessage("Сообщение не может быть пустым");
        }
    }
}