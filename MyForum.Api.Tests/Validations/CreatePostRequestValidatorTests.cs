using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.Validations;

namespace MyForum.Api.Tests.Validations
{
    public class CreatePostRequestValidatorTests
    {
        private readonly CreatePostRequestValidator _validator;

        public CreatePostRequestValidatorTests()
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

            _validator = new CreatePostRequestValidator(configuration);
        }

        [Fact]
        public void ShouldHaveErrorWhenContentIsEmpty()
        {
            // Arrange
            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "",
                AuthorName: "TestUser",
                Files: null,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content)
                .WithErrorMessage("Сообщение не может быть пустым");
        }

        [Fact]
        public void ShouldHaveErrorWhenContentTooLong()
        {
            // Arrange
            var longContent = new string('a', 2001);
            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: longContent,
                AuthorName: "TestUser",
                Files: null,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Content)
                .WithErrorMessage("Сообщение не более 2000 символов");
        }

        [Fact]
        public void ShouldHaveErrorWhenAuthorNameHasInvalidCharacters()
        {
            // Arrange
            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "Invalid@Name!",
                Files: null,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AuthorName)
                .WithErrorMessage("Недопустимые символы в имени");
        }

        [Theory]
        [InlineData("ValidName123")]
        [InlineData("Анонимный Пользователь")]
        public void ShouldNotHaveErrorWhenAuthorNameIsValid(string validName)
        {
            // Arrange
            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: validName,
                Files: null,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.AuthorName);
        }

        [Fact]
        public void ShouldHaveErrorWhenAuthorNameTooLong()
        {
            // Arrange
            var longName = new string('a', 51);
            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: longName,
                Files: null,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AuthorName)
                .WithErrorMessage("Имя не более 50 символов");
        }

        [Fact]
        public void ShouldNotHaveErrorWhenFilesIsValid()
        {
            // Arrange
            var files = new List<IFormFile>();
            for (int i = 0; i < 3; i++)
            {
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.FileName).Returns($"test{i}.jpg");
                mockFile.Setup(f => f.Length).Returns(1024 * 1024); // 1 MB
                files.Add(mockFile.Object);
            }

            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                Files: files,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Files);
        }

        [Fact]
        public void ShouldHaveErrorWhenTooManyFiles()
        {
            // Arrange
            var files = new List<IFormFile>();
            for (int i = 0; i < 5; i++)
            {
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.FileName).Returns($"test{i}.jpg");
                files.Add(mockFile.Object);
            }

            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                Files: files,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Files)
                .WithErrorMessage("Максимум 4 файла");
        }

        [Fact]
        public void ShouldHaveErrorWhenFileTooLarge()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("largefile.jpg");
            mockFile.Setup(f => f.Length).Returns(5 * 1024 * 1024); // 5 MB

            var files = new List<IFormFile> { mockFile.Object };

            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                Files: files,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor("Files[0].Length")
                .WithErrorMessage("Файл слишком большой");
        }

        [Fact]
        public void ShouldHaveErrorWhenFileHasUnsupportedType()
        {
            // Arrnge
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document.pdf");
            mockFile.Setup(f => f.Length).Returns(1024 * 1024); // 1 MB

            var files = new List<IFormFile> { mockFile.Object };

            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                Files: files,
                ReplyToPostId: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor("Files[0].FileName")
                .WithErrorMessage("Неподдерживаемый тип файла");
        }
    }
}