using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Tests.Repositories
{
    public class ThreadRepositoryTests : IDisposable
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
        public async Task GetThreadWithOriginalPostAsync_ShouldReturnThreadWithOnlyOriginalPost()
        {
            // Arrange
            var board = new Board { Name = "Technology", Description = "All about technology", ShortName = "tech", Position = 1 };
            var thread = new Thread { Subject = "First Thread", Board = board };
            var originalPost = new Post
            {
                Content = "Original",
                Thread = thread,
                IpAddressHash = "hashed",
                IsOriginal = true
            };
            var replyPost = new Post
            {
                Content = "Reply",
                Thread = thread,
                IpAddressHash = "hashed",
                IsOriginal = false
            };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.Posts.AddRangeAsync(originalPost, replyPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetThreadWithOriginalPostAsync("tech", thread.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(thread.Id, result!.Id);
            Assert.Equal("tech", result.Board.ShortName);
            Assert.Single(result.Posts);
            Assert.True(result.Posts.Single().IsOriginal);
        }

        [Fact]
        public async Task GetThreadWithPostsByIdAsync_ShouldReturnThreadWithPosts()
        {
            // Arrange
            var board = new Board { Name = "Technology", Description = "All about technology", ShortName = "tech", Position = 1 };
            var thread = new Thread { Subject = "First Thread", Board = board };
            var post1 = new Post { Content = "First Post", Thread = thread, IpAddressHash = "hashed", IsOriginal = true };
            var post2 = new Post { Content = "Second Post", Thread = thread, IpAddressHash = "hashed", IsOriginal = false };
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
            Assert.True(result.Posts.First().IsOriginal);
            Assert.Contains(result.Posts, p => p.Content == "First Post");
            Assert.Contains(result.Posts, p => p.Content == "Second Post");
        }

        [Fact]
        public async Task GetThreadWithPostsByIdAsync_ShouldRespectPostLimitBounds()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b1", Position = 1 };
            var thread = new Thread { Subject = "Thread", Board = board };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);

            var posts = Enumerable.Range(1, 3)
                .Select(i => new Post
                {
                    Thread = thread,
                    Content = $"Post {i}",
                    IpAddressHash = $"h{i}",
                    IsOriginal = i == 1,
                    CreatedAt = DateTime.UtcNow.AddMinutes(i)
                })
                .ToList();

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            // Act
            var resultWithZeroLimit = await _threadRepository.GetThreadWithPostsByIdAsync("b1", thread.Id, 0);
            var resultWithHugeLimit = await _threadRepository.GetThreadWithPostsByIdAsync("b1", thread.Id, 9999);

            // Assert
            Assert.NotNull(resultWithZeroLimit);
            Assert.Single(resultWithZeroLimit!.Posts);
            Assert.NotNull(resultWithHugeLimit);
            Assert.Equal(3, resultWithHugeLimit!.Posts.Count);
        }

        [Fact]
        public async Task GetThreadsByCursorWithPostsAsync_ShouldReturnOnlyNotPinnedThreadsAndUpToFourPosts()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b2", Position = 1 };
            var pinnedThread = new Thread { Subject = "Pinned", Board = board, IsPinned = true, LastBumpAt = DateTime.UtcNow.AddMinutes(-5) };
            var regularThread = new Thread { Subject = "Regular", Board = board, IsPinned = false, LastBumpAt = DateTime.UtcNow };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddRangeAsync(pinnedThread, regularThread);
            await _context.SaveChangesAsync();

            var posts = Enumerable.Range(1, 5)
                .Select(i => new Post
                {
                    ThreadId = regularThread.Id,
                    Content = $"Post {i}",
                    IpAddressHash = $"h{i}",
                    IsOriginal = i == 1,
                    CreatedAt = DateTime.UtcNow.AddMinutes(i)
                })
                .ToList();

            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetThreadsByCursorWithPostsAsync("b2", cursor: null, limit: 10);

            // Assert
            Assert.Single(result);
            Assert.Equal(regularThread.Id, result[0].Id);
            Assert.Equal(4, result[0].Posts.Count);
            Assert.True(result[0].Posts.First().IsOriginal);
        }

        [Fact]
        public async Task GetThreadsByCursorWithPostsAsync_WithCursor_ShouldReturnOlderThreadsOnly()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b3", Position = 1 };
            var olderThread = new Thread { Subject = "Older", Board = board, LastBumpAt = DateTime.UtcNow.AddHours(-2) };
            var newerThread = new Thread { Subject = "Newer", Board = board, LastBumpAt = DateTime.UtcNow };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddRangeAsync(olderThread, newerThread);
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetThreadsByCursorWithPostsAsync("b3", cursor: DateTime.UtcNow.AddHours(-1), limit: 10);

            // Assert
            Assert.Single(result);
            Assert.Equal(olderThread.Id, result[0].Id);
        }

        [Fact]
        public async Task GetThreadsAsync_ShouldReturnDeletedAndApplyFiltersAndOrderByLastBumpAt()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b4", Position = 1 };
            var matchingThread1 = new Thread { Subject = "A", Board = board, IsLocked = true, IsDeleted = true, LastBumpAt = DateTime.UtcNow.AddMinutes(-1) };
            var matchingThread2 = new Thread { Subject = "B", Board = board, IsLocked = true, IsDeleted = true, LastBumpAt = DateTime.UtcNow.AddMinutes(-3) };
            var notDeletedThread = new Thread { Subject = "C", Board = board, IsLocked = true, IsDeleted = false, LastBumpAt = DateTime.UtcNow.AddMinutes(-2) };
            var notLockedThread = new Thread { Subject = "D", Board = board, IsLocked = false, IsDeleted = true, LastBumpAt = DateTime.UtcNow.AddMinutes(-4) };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddRangeAsync(matchingThread1, matchingThread2, notDeletedThread, notLockedThread);
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetThreadsAsync(
                limit: 10,
                cursor: null,
                search: null,
                board: null,
                isDeleted: true,
                isLocked: true);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.True(t.IsDeleted));
            Assert.All(result, t => Assert.True(t.IsLocked));
            Assert.Equal(new[] { matchingThread1.Id, matchingThread2.Id }, result.Select(t => t.Id));
        }

        [Fact]
        public async Task GetThreadsAsync_ShouldRespectLimitBoundsAndCursor()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b5", Position = 1 };
            var t1 = new Thread { Subject = "T1", Board = board, LastBumpAt = DateTime.UtcNow.AddHours(-1) };
            var t2 = new Thread { Subject = "T2", Board = board, LastBumpAt = DateTime.UtcNow.AddHours(-2) };
            var t3 = new Thread { Subject = "T3", Board = board, LastBumpAt = DateTime.UtcNow.AddHours(-3) };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddRangeAsync(t1, t2, t3);
            await _context.SaveChangesAsync();

            // Act
            var resultWithZeroLimit = await _threadRepository.GetThreadsAsync(limit: 0, cursor: null);
            var resultWithHugeLimitAndCursor = await _threadRepository.GetThreadsAsync(
                limit: 9999,
                cursor: DateTime.UtcNow.AddHours(-1).AddMinutes(-30));

            // Assert
            Assert.Single(resultWithZeroLimit);
            Assert.Equal(2, resultWithHugeLimitAndCursor.Count);
            Assert.DoesNotContain(resultWithHugeLimitAndCursor, t => t.Id == t1.Id);
        }

        [Fact]
        public async Task GetByIdIncludingDeletedAsync_ShouldReturnDeletedThread()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b6", Position = 1 };
            var deletedThread = new Thread { Subject = "Deleted", Board = board, IsDeleted = true };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(deletedThread);
            await _context.SaveChangesAsync();

            // Act
            var result = await _threadRepository.GetByIdIncludingDeletedAsync(deletedThread.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.IsDeleted);
            Assert.Equal(deletedThread.Subject, result.Subject);
        }

        [Fact]
        public async Task RecountThreadStatsAsync_ShouldCountOnlyNotDeletedPosts()
        {
            // Arrange
            var board = new Board { Name = "Board", Description = "desc", ShortName = "b7", Position = 1 };
            var thread = new Thread { Subject = "Thread", Board = board };

            await _context.Boards.AddAsync(board);
            await _context.Threads.AddAsync(thread);
            await _context.SaveChangesAsync();

            var original = new Post { ThreadId = thread.Id, Content = "Original", IpAddressHash = "h1", IsOriginal = true, IsDeleted = false };
            var replyWithTwoFiles = new Post { ThreadId = thread.Id, Content = "Reply 1", IpAddressHash = "h2", IsOriginal = false, IsDeleted = false };
            var deletedReply = new Post { ThreadId = thread.Id, Content = "Reply 2", IpAddressHash = "h3", IsOriginal = false, IsDeleted = true };

            await _context.Posts.AddRangeAsync(original, replyWithTwoFiles, deletedReply);
            await _context.SaveChangesAsync();

            await _context.PostFiles.AddRangeAsync(
                new PostFile { PostId = original.Id, FileName = "f1.png", StoredFileName = "sf1.png" },
                new PostFile { PostId = replyWithTwoFiles.Id, FileName = "f2.png", StoredFileName = "sf2.png" },
                new PostFile { PostId = replyWithTwoFiles.Id, FileName = "f3.png", StoredFileName = "sf3.png" },
                new PostFile { PostId = deletedReply.Id, FileName = "f4.png", StoredFileName = "sf4.png" }
            );
            await _context.SaveChangesAsync();

            // Act
            var stats = await _threadRepository.RecountThreadStatsAsync(thread.Id);

            // Assert
            Assert.Equal(2, stats.PostCount);
            Assert.Equal(3, stats.FileCount);
            Assert.Equal(1, stats.ReplyCount);
        }

        public void Dispose()
        {
            DbContext.Dispose(_context);
        }
    }
}
