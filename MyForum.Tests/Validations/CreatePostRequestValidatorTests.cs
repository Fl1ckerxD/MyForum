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
        public void Should_Have_Error_When_Content_Is_Empty()
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
        public void Should_Have_Error_When_Content_Too_Long()
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
        public void Should_Have_Error_When_Too_Many_Files()
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
    }
}