using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;

namespace MyForum.Api.Tests.Repositories
{
    public class BanRepositoryTests : IDisposable
    {
        private readonly BanRepository _banRepository;
        private readonly ForumDbContext _context;

        public BanRepositoryTests()
        {
            var options = DbContext.GetOptions(nameof(BanRepositoryTests));
            _context = new ForumDbContext(options);
            _banRepository = new BanRepository(_context);
        }

        [Fact]
        public async Task IsBannedAsync_ShouldReturnTrue_ForActiveGlobalBan()
        {
            // Arrange
            var ban = new Ban
            {
                IpAddressHash = "ip1",
                Reason = "Global",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                BoardId = null
            };
            await _context.Bans.AddAsync(ban);
            await _context.SaveChangesAsync();

            // Act
            var result = await _banRepository.IsBannedAsync("ip1", boardId: 123, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsBannedAsync_ShouldReturnTrue_ForBoardSpecificBanOnSameBoard()
        {
            // Arrange
            var board = new Board { Name = "Technology", Description = "desc", ShortName = "tech", Position = 1 };
            await _context.Boards.AddAsync(board);
            await _context.SaveChangesAsync();

            var ban = new Ban
            {
                IpAddressHash = "ip2",
                Reason = "Board specific",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                BoardId = board.Id
            };
            await _context.Bans.AddAsync(ban);
            await _context.SaveChangesAsync();

            // Act
            var result = await _banRepository.IsBannedAsync("ip2", board.Id, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsBannedAsync_ShouldReturnFalse_ForDifferentBoardOrExpiredOrRevoked()
        {
            // Arrange
            var board1 = new Board { Name = "Board1", Description = "desc", ShortName = "b1", Position = 1 };
            var board2 = new Board { Name = "Board2", Description = "desc", ShortName = "b2", Position = 2 };
            await _context.Boards.AddRangeAsync(board1, board2);
            await _context.SaveChangesAsync();

            await _context.Bans.AddRangeAsync(
                new Ban
                {
                    IpAddressHash = "ip3",
                    Reason = "Another board",
                    IsActive = true,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    BoardId = board1.Id
                },
                new Ban
                {
                    IpAddressHash = "ip4",
                    Reason = "Expired",
                    IsActive = true,
                    ExpiresAt = DateTime.UtcNow.AddHours(-1),
                    BoardId = null
                },
                new Ban
                {
                    IpAddressHash = "ip5",
                    Reason = "Revoked",
                    IsActive = false,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    BoardId = null
                });
            await _context.SaveChangesAsync();

            // Act
            var differentBoardResult = await _banRepository.IsBannedAsync("ip3", board2.Id, CancellationToken.None);
            var expiredResult = await _banRepository.IsBannedAsync("ip4", boardId: null, CancellationToken.None);
            var revokedResult = await _banRepository.IsBannedAsync("ip5", boardId: null, CancellationToken.None);

            // Assert
            Assert.False(differentBoardResult);
            Assert.False(expiredResult);
            Assert.False(revokedResult);
        }

        [Fact]
        public async Task GetBansAsync_ShouldReturnOrderedAndFilteredByStatus()
        {
            // Arrange
            await _context.Bans.AddRangeAsync(
                new Ban { IpAddressHash = "ipA", Reason = "Active", IsActive = true, ExpiresAt = DateTime.UtcNow.AddHours(2) },
                new Ban { IpAddressHash = "ipB", Reason = "Expired", IsActive = true, ExpiresAt = DateTime.UtcNow.AddHours(-2) },
                new Ban { IpAddressHash = "ipC", Reason = "Revoked", IsActive = false, ExpiresAt = DateTime.UtcNow.AddHours(2) });
            await _context.SaveChangesAsync();

            // Act
            var active = await _banRepository.GetBansAsync(status: "active");
            var expired = await _banRepository.GetBansAsync(status: "expired");
            var revoked = await _banRepository.GetBansAsync(status: "revoked");

            // Assert
            Assert.Single(active);
            Assert.True(active[0].IsActive);
            Assert.True(active[0].ExpiresAt == null || active[0].ExpiresAt > DateTime.UtcNow);

            Assert.Single(expired);
            Assert.True(expired[0].IsActive);
            Assert.True(expired[0].ExpiresAt <= DateTime.UtcNow);

            Assert.Single(revoked);
            Assert.False(revoked[0].IsActive);
        }

        [Fact]
        public async Task GetBansAsync_ShouldFilterByBoardShortNameAndBeforeId()
        {
            // Arrange
            var boardA = new Board { Name = "BoardA", Description = "desc", ShortName = "a", Position = 1 };
            var boardB = new Board { Name = "BoardB", Description = "desc", ShortName = "b", Position = 2 };
            await _context.Boards.AddRangeAsync(boardA, boardB);
            await _context.SaveChangesAsync();

            var ban1 = new Ban { IpAddressHash = "ip1", Reason = "R1", BoardId = boardA.Id };
            var ban2 = new Ban { IpAddressHash = "ip2", Reason = "R2", BoardId = boardA.Id };
            var ban3 = new Ban { IpAddressHash = "ip3", Reason = "R3", BoardId = boardB.Id };

            await _context.Bans.AddRangeAsync(ban1, ban2, ban3);
            await _context.SaveChangesAsync();

            // Act
            var byBoard = await _banRepository.GetBansAsync(boardShortName: "a");
            var beforeId = await _banRepository.GetBansAsync(beforeId: ban3.Id);

            // Assert
            Assert.Equal(2, byBoard.Count);
            Assert.All(byBoard, b =>
            {
                Assert.NotNull(b.Board);
                Assert.Equal("a", b.Board!.ShortName);
            });

            Assert.Equal(2, beforeId.Count);
            Assert.DoesNotContain(beforeId, b => b.Id >= ban3.Id);
        }

        [Fact]
        public async Task GetBansAsync_ShouldRespectLimitBounds()
        {
            // Arrange
            await _context.Bans.AddRangeAsync(
                new Ban { IpAddressHash = "ip1", Reason = "R1" },
                new Ban { IpAddressHash = "ip2", Reason = "R2" },
                new Ban { IpAddressHash = "ip3", Reason = "R3" });
            await _context.SaveChangesAsync();

            // Act
            var resultWithZeroLimit = await _banRepository.GetBansAsync(limit: 0);
            var resultWithHugeLimit = await _banRepository.GetBansAsync(limit: 9999);

            // Assert
            Assert.Single(resultWithZeroLimit);
            Assert.Equal(3, resultWithHugeLimit.Count);
        }

        [Fact]
        public async Task GetBansAsync_WithInvalidStatus_ShouldThrowArgumentException()
        {
            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _banRepository.GetBansAsync(status: "unknown"));
        }

        public void Dispose()
        {
            DbContext.Dispose(_context);
        }
    }
}
