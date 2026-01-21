using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Factories;
using MyForum.Api.Core.Interfaces.Metrics;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Infrastructure.Services;

namespace MyForum.Api.Tests.Services
{
    public class ThreadServiceTests
    {
        private readonly Mock<ILogger<ThreadService>> _mockLogger;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ThreadService _threadService;
        private readonly Mock<IPostService> _mockPostService;
        private readonly Mock<IForumMetrics> _mockForumMetrics;
        private readonly Mock<IThreadDtoFactory> _mockThreadDtoFactory;
        public ThreadServiceTests()
        {
            _mockLogger = new Mock<ILogger<ThreadService>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockPostService = new Mock<IPostService>();
            _mockForumMetrics = new Mock<IForumMetrics>();
            _mockThreadDtoFactory = new Mock<IThreadDtoFactory>();

            _threadService = new ThreadService(
                _mockLogger.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockPostService.Object,
                _mockForumMetrics.Object,
                _mockThreadDtoFactory.Object);
        }

        [Fact]
        public async Task GetThreadsPagedAsync_BoardNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            string boardShortName = "nonexistent-board";
            int pageNumber = 1;
            int pageSize = 10;

            _mockUnitOfWork.Setup(uow => uow.Boards.GetByShortNameAsync(boardShortName, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Board?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _threadService.GetThreadsPagedAsync(boardShortName, pageNumber, pageSize));
        }
    }
}