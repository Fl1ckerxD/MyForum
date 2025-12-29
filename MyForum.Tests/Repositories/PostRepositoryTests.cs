using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Tests.Repositories
{
    public class PostRepositoryTests
    {
        private readonly PostRepository _postRepository;
        private readonly ForumDbContext _context;

        public PostRepositoryTests()
        {
            var options = DbContext.GetOptions(nameof(ThreadRepositoryTests));
            _context = new ForumDbContext(options);
            _postRepository = new PostRepository(_context);
        }

        [Fact]
        public async Task GetPagedPostsByThreadIdAsync_ShouldReturnPagedPosts()
        {
            // Arrange
            var board = new Board { Name = "Science", Description = "All about science", ShortName = "sci", Position = 1 };
            var thread = new Thread { Subject = "Science Thread", Board = board };
            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);

            for (int i = 1; i <= 12; i++)
            {
                var post = new Post { Content = $"Post {i}", Thread = thread, CreatedAt = DateTime.UtcNow.AddMinutes(i) };
                await _context.Posts.AddAsync(post);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPagedPostsByThreadIdAsync(thread.Id, 2, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(12, result.TotalCount);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(5, result.PageSize);
            Assert.Equal("Post 6", result.Items.First().Content);
            Assert.Equal("Post 10", result.Items.Last().Content);
        }
    }
}