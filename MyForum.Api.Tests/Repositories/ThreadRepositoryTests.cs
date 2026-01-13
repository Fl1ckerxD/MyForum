using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Tests.Repositories
{
    public class ThreadRepositoryTests
    {
        private readonly ThreadRepository _threadRepository;
        private readonly ForumDbContext _context;

        public ThreadRepositoryTests()
        {
            var options = DbContext.GetOptions(nameof(ThreadRepositoryTests));
            _context = new ForumDbContext(options);
            _threadRepository = new ThreadRepository(_context);
        }

        [Fact]
        public async Task GetThreadWithPostsById_ShouldReturnThreadWithPosts()
        {
            // Arrange
            var board = new Board { Name = "Technology", Description = "All about technology", ShortName = "tech", Position = 1 };
            var thread = new Thread { Subject = "First Thread", Board = board };
            var post1 = new Post { Content = "First Post", Thread = thread, IpAddressHash = "hashed" };
            var post2 = new Post { Content = "Second Post", Thread = thread, IpAddressHash = "hashed" };
            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.Posts.AddRangeAsync(post1, post2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetThreadWithPostsByIdAsync("tech", thread.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("First Thread", result!.Subject);
            Assert.Equal(2, result.Posts.Count);
            Assert.Contains(result.Posts, p => p.Content == "First Post");
            Assert.Contains(result.Posts, p => p.Content == "Second Post");
        }

        [Fact]
        public async Task GetPagedThreadsByBoardShortNameAsync_ShouldReturnPagedThreads()
        {
            // Arrange
            var board = new Board { Name = "General", Description = "General Discussion", ShortName = "gen", Position = 1 };
            await _context.Boards.AddAsync(board);

            for (int i = 1; i <= 15; i++)
            {
                var thread = new Thread { Subject = $"Thread {i}", Board = board, IsPinned = i <= 2, LastBumpAt = DateTime.UtcNow.AddMinutes(-i) };
                await _context.Threads.AddAsync(thread);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetPagedThreadsByBoardShortNameAsync("gen", 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(15, result.TotalCount);
            Assert.True(result.Items[0].IsPinned);
            Assert.True(result.Items[1].IsPinned);
        }

        [Fact]
        public async Task GetPagedThreadsByBoardWithPostsAsync_ShouldReturnPagedThreadsWithPosts()
        {
            // Arrange
            var board = new Board { Name = "News", Description = "Latest News", ShortName = "news", Position = 1 };
            await _context.Boards.AddAsync(board);

            for (int i = 1; i <= 10; i++)
            {
                var thread = new Thread { Subject = $"News Thread {i}", Board = board, LastBumpAt = DateTime.UtcNow.AddMinutes(-i) };
                await _context.Threads.AddAsync(thread);

                for (int j = 1; j <= 3; j++)
                {
                    var post = new Post { Content = $"Post {j} in Thread {i}", Thread = thread, IpAddressHash = "hashed" };
                    await _context.Posts.AddAsync(post);
                }
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetPagedThreadsByBoardWithPostsAsync(board.Id, 1, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(10, result.TotalCount);
            result.Items.ForEach(t =>
            {
                Assert.True(t.Posts.Count <= 3);
            });
        }
    }
}