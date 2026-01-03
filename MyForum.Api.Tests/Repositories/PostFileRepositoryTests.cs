using MyForum.Api.Core.Entities;
using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;

namespace MyForum.Api.Tests.Repositories
{
    public class PostFileRepositoryTests
    {
        private readonly PostFileRepository _postFileRepository;
        private readonly ForumDbContext _context;

        public PostFileRepositoryTests()
        {
            var options = DbContext.GetOptions(nameof(PostFileRepositoryTests));
            _context = new ForumDbContext(options);
            _postFileRepository = new PostFileRepository(_context);
        }

        [Fact]
        public async Task GetByPostIdAsync_ShouldReturnPostFilesForGivenPostId()
        {
            // Arrange
            var post = new Post { Content = "Sample Post" };
            var postFile1 = new PostFile { FileName = "file1.png", StoredFileName = "stored_file1.png", Post = post };
            var postFile2 = new PostFile { FileName = "file2.png", StoredFileName = "stored_file2.png", Post = post };
            await _context.Posts.AddAsync(post);
            await _context.PostFiles.AddRangeAsync(postFile1, postFile2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postFileRepository.GetByPostIdAsync(post.Id);

            // Assert
            Assert.NotNull(result);
            var postFilesList = result.ToList();
            Assert.Equal(2, postFilesList.Count);
            Assert.Contains(postFilesList, pf => pf.FileName == "file1.png");
            Assert.Contains(postFilesList, pf => pf.FileName == "file2.png");
        }
    }
}