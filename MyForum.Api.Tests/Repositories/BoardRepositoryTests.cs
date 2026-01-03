using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Tests.Repositories
{
    public class BoardRepositoryTests : IDisposable
    {
        private readonly BoardRepository _boardRepository;
        private readonly ForumDbContext _context;

        public BoardRepositoryTests()
        {
            var options = DbContext.GetOptions(nameof(BoardRepositoryTests));
            _context = new ForumDbContext(options);
            _boardRepository = new BoardRepository(_context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnBoardsInCorrectOrder()
        {
            // Arrange
            var board1 = new Board { Name = "Technology", Description = "All about technology", ShortName = "tech", Position = 2 };
            var board2 = new Board { Name = "Games", Description = "Gaming discussions", ShortName = "games", Position = 1 };
            await _context.Boards.AddRangeAsync(board1, board2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _boardRepository.GetAllAsync();
            var resultList = result.ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.Equal("games", resultList[0].ShortName);
            Assert.Equal("tech", resultList[1].ShortName);
        }

        [Fact]
        public async Task GetBoardWithThreadsAndPostsAsync_ShouldReturnBoardWithThreadsAndPosts()
        {
            // Arrange
            var board = new Board { Name = "Technology", Description = "All about technology", ShortName = "tech", Position = 1 };
            var thread = new Thread { Subject = "First Thread", Board = board };
            var post = new Post { Content = "First Post", Thread = thread };
            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _boardRepository.GetBoardWithThreadsAndPostsAsync("tech");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tech", result.ShortName);
            Assert.Single(result.Threads);
            Assert.Single(result.Threads.First().Posts);
            Assert.Equal("First Post", result.Threads.First().Posts.First().Content);
        }

        public void Dispose()
        {
            DbContext.Dispose(_context);
        }
    }
}