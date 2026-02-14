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
            var result = await _threadRepository.GetThreadWithPostsByIdAsync("tech", thread.Id, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("First Thread", result!.Subject);
            Assert.Equal(2, result.Posts.Count);
            Assert.Contains(result.Posts, p => p.Content == "First Post");
            Assert.Contains(result.Posts, p => p.Content == "Second Post");
        }
    }
}