using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Metrics;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Infrastructure.Services;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Tests.Services
{
    public class PostServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<PostService>> _mockLogger;
        private readonly Mock<IObjectStorageService> _mockFileService;
        private readonly Mock<IIPHasher> _mockIpHasher;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IForumMetrics> _mockForumMetrics;
        private readonly PostService _postService;
        public PostServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<PostService>>();
            _mockFileService = new Mock<IObjectStorageService>();
            _mockIpHasher = new Mock<IIPHasher>();
            _mockMapper = new Mock<IMapper>();
            _mockForumMetrics = new Mock<IForumMetrics>();

            _mockIpHasher.Setup(hasher => hasher.HashIP(It.IsAny<string>())).Returns("hashed_ip");

            _postService = new PostService(
                _mockLogger.Object,
                _mockUnitOfWork.Object,
                _mockFileService.Object,
                _mockIpHasher.Object,
                _mockMapper.Object,
                _mockForumMetrics.Object);

        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreatePost()
        {
            // Arrange
            var threadId = 1;
            var content = "Test content";
            var authorName = "Test Author";
            var ipAddress = "192.168.1.1";
            var userAgent = "UnitTestAgent";

            var mockPostRepo = new Mock<IPostRepository>();

            _mockUnitOfWork.Setup(uow => uow.Posts).Returns(mockPostRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _postService.CreateAsync(threadId, content, authorName, ipAddress, userAgent, null);

            // Assert
            mockPostRepo.Verify(repo => repo.AddAsync(It.Is<Post>(p =>
                p.ThreadId == threadId &&
                p.Content == content &&
                p.AuthorName == authorName &&
                p.IpAddressHash == "hashed_ip" &&
                p.UserAgent == userAgent
            ), It.IsAny<CancellationToken>()), Times.Once);

            _mockUnitOfWork.Verify(uow => uow.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithFiles_ShouldProcessFiles()
        {
            // Arrange
            var threadId = 1;
            var content = "Test content";
            var authorName = "Test Author";
            var ipAddress = "192.168.1.1";
            var userAgent = "UnitTestAgent";

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

            var files = new List<IFormFile> { mockFile.Object };

            var mockPostRepo = new Mock<IPostRepository>();
            var mockPostFileRepo = new Mock<IPostFileRepository>();

            _mockUnitOfWork.Setup(uow => uow.Posts).Returns(mockPostRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var postFile = new PostFile { Id = 1, FileName = "test.jpg" };
            _mockFileService.Setup(fs => fs.SaveFileAsync(mockFile.Object, It.IsAny<Post>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(postFile);

            // Act
            await _postService.CreateAsync(threadId, content, authorName, ipAddress, userAgent, files);

            // Assert
            mockPostRepo.Verify(repo => repo.AddAsync(It.Is<Post>(p =>
                p.ThreadId == threadId &&
                p.Content == content &&
                p.AuthorName == authorName &&
                p.IpAddressHash == "hashed_ip" &&
                p.UserAgent == userAgent
            ), It.IsAny<CancellationToken>()), Times.Once);

            _mockFileService.Verify(fs => fs.SaveFileAsync(mockFile.Object, It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_AsOriginalPost_ShouldCreatePostWithThread()
        {
            // Arrange
            var thread = new Thread
            {
                Subject = "Test Thread",
                BoardId = 1
            };

            var postId = 123;
            var content = "Test content";
            var authorName = "Test Author";
            var ipAddress = "192.168.1.1";
            var userAgent = "UnitTestAgent";

            var mockPostRepo = new Mock<IPostRepository>();

            _mockUnitOfWork.Setup(uow => uow.Posts).Returns(mockPostRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            mockPostRepo.Setup(repo => repo.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
                .Callback<Post, CancellationToken>((post, ct) => post.Id = postId);

            // Act
            await _postService.CreateAsync(thread, content, authorName, ipAddress, userAgent, null);

            // Assert
            mockPostRepo.Verify(repo => repo.AddAsync(It.Is<Post>(p =>
                p.Thread == thread &&
                p.Content == content &&
                p.AuthorName == authorName &&
                p.IpAddressHash == "hashed_ip" &&
                p.UserAgent == userAgent
            ), It.IsAny<CancellationToken>()), Times.Once);

            _mockUnitOfWork.Verify(uow => uow.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
