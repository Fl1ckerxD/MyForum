using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using MyForum.Core.DTOs.Requests;
using MyForum.Core.Validations;

namespace MyForum.Tests.Validations
{
    public class CreatePostRequestValidatorTests
    {
        private readonly CreatePostRequestValidator _validator;

        public CreatePostRequestValidatorTests()
        {
            var inMemory = new Dictionary<string, string?>
            {
                ["MinIO:MaxFilesPerPost"] = "4",
                ["MinIO:MaxFileSize"] = (4 * 1024 * 1024).ToString(),
                ["MinIO:AllowedExtensions:0"] = ".jpg",
                ["MinIO:AllowedExtensions:1"] = ".png"
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
                PostPassword: "password",
                Files: null
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
                PostPassword: "password",
                Files: null
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
                PostPassword: "password",
                Files: null
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
                PostPassword: "password",
                Files: null
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
                PostPassword: "password",
                Files: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AuthorName)
                .WithErrorMessage("Имя не более 50 символов");
        }

        [Fact]
        public void ShouldHaveErrorWhenPostPasswordTooLong()
        {
            // Arrange
            var longPassword = new string('a', 21);
            var model = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                PostPassword: longPassword,
                Files: null
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PostPassword)
                .WithErrorMessage("Пароль не более 20 символов");
        }

        [Fact]
        public void ShouldNotHaveErrorWhenPostPasswordIsNullOrEmpty()
        {
            // Arrange
            var modelWithNullPassword = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                PostPassword: null,
                Files: null
            );

            var modelWithEmptyPassword = new CreatePostRequest(
                ThreadId: 1,
                Content: "Test content",
                AuthorName: "TestUser",
                PostPassword: "",
                Files: null
            );

            // Act
            var resultWithNullPassword = _validator.TestValidate(modelWithNullPassword);
            var resultWithEmptyPassword = _validator.TestValidate(modelWithEmptyPassword);

            // Assert
            resultWithNullPassword.ShouldNotHaveValidationErrorFor(x => x.PostPassword);
            resultWithEmptyPassword.ShouldNotHaveValidationErrorFor(x => x.PostPassword);
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
                PostPassword: "password",
                Files: files
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
                PostPassword: "password",
                Files: files
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
                PostPassword: "password",
                Files: files
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
                PostPassword: "password",
                Files: files
            );

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor("Files[0].FileName")
                .WithErrorMessage("Неподдерживаемый тип файла");
        }
    }
}