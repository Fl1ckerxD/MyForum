using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;
using MyForum.Infrastructure.Services;

namespace MyForum.Tests.Services
{
    public class ThreadServiceTests
    {
        private readonly Mock<ILogger<ThreadService>> _mockLogger;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ThreadService _threadService;
        private readonly Mock<IPostService> _mockPostService;
        public ThreadServiceTests()
        {
            _mockLogger = new Mock<ILogger<ThreadService>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockPostService = new Mock<IPostService>();

            _threadService = new ThreadService(
                _mockLogger.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockPostService.Object);

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