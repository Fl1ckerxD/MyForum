using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Moq;
using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MyForum.Api.Tests.Services
{
    public class MinioFileServiceTests
    {
        private readonly Mock<IMinioClient> _mockMinioClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<MinioObjectStorageService>> _mockLogger;
        private readonly MinioObjectStorageService _fileService;

        public MinioFileServiceTests()
        {
            _mockMinioClient = new Mock<IMinioClient>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<MinioObjectStorageService>>();

            _fileService = new MinioObjectStorageService(
                _mockMinioClient.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task SaveFileAsync_WithImageFile_ShouldCreatePostFileWithThumbnail()
        {
            // Arrange
            var post = new Post();
            var fileName = "test.jpg";

            // Создаем тестовое изображение в памяти
            using var memoryStream = new MemoryStream();
            using (var image = new Image<Rgba32>(800, 600))
            {
                await image.SaveAsJpegAsync(memoryStream);
            }
            memoryStream.Position = 0;

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Настройка MinIO клиента
            _mockMinioClient.Setup(m => m.BucketExistsAsync(
                It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMinioClient.Setup(m => m.PutObjectAsync(
                It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PutObjectResponse)null);

            // Act
            var result = await _fileService.SaveFileAsync(mockFile.Object, post);

            // Assert
            result.Should().NotBeNull();
            result.FileName.Should().Be(fileName);
            result.Post.Should().Be(post);
            result.Width.Should().Be(800);
            result.Height.Should().Be(600);
            result.ThumbnailKey.Should().NotBeNullOrEmpty();
        }
    }
}