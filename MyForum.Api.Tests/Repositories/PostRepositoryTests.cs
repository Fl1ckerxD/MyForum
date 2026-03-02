using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;

namespace MyForum.Api.Tests.Repositories
{
    public class PostRepositoryTests : IDisposable
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
        public async Task GetByIdIncludingDeletedAsync_ShouldReturnDeletedPost()
        {
            // Arrange
            var deletedPost = new Post
            {
                ThreadId = 1,
                Content = "Deleted Post",
                AuthorName = "John Doe",
                IpAddressHash = "hashed",
                IsDeleted = true
            };

            await _context.Posts.AddAsync(deletedPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetByIdIncludingDeletedAsync(deletedPost.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(deletedPost.Content, result.Content);
            Assert.Equal(deletedPost.AuthorName, result.AuthorName);
            Assert.Equal(deletedPost.IpAddressHash, result.IpAddressHash);
            Assert.True(result.IsDeleted);
        }

        [Fact]
        public async Task GetByThreadIncludingDeletedAsync_ShouldReturnBothDeletedAndNotDeletedOrderedById()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b", Position = 1 };
            var thread = new Core.Entities.Thread { Subject = "Thread", Board = board };
            var deletedPost = new Post { Thread = thread, Content = "Deleted", IpAddressHash = "h1", IsDeleted = true };
            var activePost = new Post { Thread = thread, Content = "Active", IpAddressHash = "h2", IsDeleted = false };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.Posts.AddRangeAsync(deletedPost, activePost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetByThreadIncludingDeletedAsync(thread.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(new[] { deletedPost.Id, activePost.Id }.OrderBy(x => x), result.Select(p => p.Id));
            Assert.Contains(result, p => p.IsDeleted);
            Assert.Contains(result, p => !p.IsDeleted);
        }

        [Fact]
        public async Task GetByThreadIncludingDeletedAsync_WithAfterIdAndIsDeleted_ShouldFilterCorrectly()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b2", Position = 1 };
            var thread = new Core.Entities.Thread { Subject = "Thread", Board = board };
            var post1 = new Post { Thread = thread, Content = "P1", IpAddressHash = "h1", IsDeleted = false };
            var post2 = new Post { Thread = thread, Content = "P2", IpAddressHash = "h2", IsDeleted = true };
            var post3 = new Post { Thread = thread, Content = "P3", IpAddressHash = "h3", IsDeleted = true };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.Posts.AddRangeAsync(post1, post2, post3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetByThreadIncludingDeletedAsync(
                thread.Id,
                limit: 50,
                afterId: post1.Id,
                search: null,
                isDeleted: true);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.True(p.Id > post1.Id));
            Assert.All(result, p => Assert.True(p.IsDeleted));
            Assert.Equal(new[] { post2.Id, post3.Id }, result.Select(p => p.Id));
        }

        [Fact]
        public async Task GetByThreadIncludingDeletedAsync_ShouldRespectLimitBounds()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b3", Position = 1 };
            var thread = new Core.Entities.Thread { Subject = "Thread", Board = board };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);

            var posts = Enumerable.Range(1, 3)
                .Select(i => new Post { Thread = thread, Content = $"P{i}", IpAddressHash = $"h{i}" })
                .ToList();

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            // Act
            var resultWithZeroLimit = await _postRepository.GetByThreadIncludingDeletedAsync(thread.Id, limit: 0);
            var resultWithHugeLimit = await _postRepository.GetByThreadIncludingDeletedAsync(thread.Id, limit: 9999);

            // Assert
            Assert.Single(resultWithZeroLimit);
            Assert.Equal(3, resultWithHugeLimit.Count);
        }

        [Fact]
        public async Task GetPostsAfterIdAsync_ShouldReturnOnlyNotDeletedPostsAfterGivenIdAndIncludeFiles()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b4", Position = 1 };
            var thread = new Core.Entities.Thread { Subject = "Thread", Board = board };
            var deletedPost = new Post { Thread = thread, Content = "Deleted", IpAddressHash = "h1", IsDeleted = true, CreatedAt = DateTime.UtcNow.AddMinutes(1) };
            var visiblePost1 = new Post { Thread = thread, Content = "Visible 1", IpAddressHash = "h2", IsDeleted = false, CreatedAt = DateTime.UtcNow.AddMinutes(2) };
            var visiblePost2 = new Post { Thread = thread, Content = "Visible 2", IpAddressHash = "h3", IsDeleted = false, CreatedAt = DateTime.UtcNow.AddMinutes(3) };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.Posts.AddRangeAsync(deletedPost, visiblePost1, visiblePost2);
            await _context.SaveChangesAsync();

            var file = new PostFile { PostId = visiblePost2.Id, FileName = "file.png", StoredFileName = "stored_file.png" };
            await _context.PostFiles.AddAsync(file);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postRepository.GetPostsAfterIdAsync(thread.Id, deletedPost.Id, limit: 20);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, p => p.IsDeleted);
            Assert.Equal(new[] { visiblePost1.Id, visiblePost2.Id }, result.Select(p => p.Id));
            var postWithFile = result.Single(p => p.Id == visiblePost2.Id);
            Assert.Single(postWithFile.Files);
        }

        [Fact]
        public async Task GetPostsAfterIdAsync_ShouldRespectLimitBounds()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b5", Position = 1 };
            var thread = new Core.Entities.Thread { Subject = "Thread", Board = board };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);

            var posts = Enumerable.Range(1, 3)
                .Select(i => new Post
                {
                    Thread = thread,
                    Content = $"P{i}",
                    IpAddressHash = $"h{i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(i)
                })
                .ToList();

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            // Act
            var resultWithZeroLimit = await _postRepository.GetPostsAfterIdAsync(thread.Id, afterId: 0, limit: 0);
            var resultWithHugeLimit = await _postRepository.GetPostsAfterIdAsync(thread.Id, afterId: 0, limit: 9999);

            // Assert
            Assert.Single(resultWithZeroLimit);
            Assert.Equal(3, resultWithHugeLimit.Count);
        }

        public void Dispose()
        {
            DbContext.Dispose(_context);
        }
    }
}
