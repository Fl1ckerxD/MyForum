using MyForum.Core.Entities;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Services.PostServices;
using MyForum.Services;

namespace MyForum.Tests.Services.PostServices
{
    public class PostServiceTests
    {
        private readonly ForumContext _context;
        private readonly PostService _postService;
        public PostServiceTests()
        {
            var options = DbContext.GetOptions();
            _context = new ForumContext(options);

            var entityService = new EntityService(_context);
            _postService = new PostService(_context, entityService);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAddCommentToDatabase()
        {
            // Arrange
            var postContent = "SampleTestContentForMyTest";
            var topicId = 1;
            var userId = 1;

            // Act
            await _postService.AddCommentAsync(topicId, postContent, userId);
            var result = _context.Posts.FirstOrDefault(p => p.Content == postContent);

            // Assert
            Assert.NotNull(_context.Posts);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldCallDeleteEntityAsync()
        {
            // Arrange
            var post = new Post
            {
                TopicId = 1,
                Content = "Sample",
                UserId = 1
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            await _postService.DeletePostAsync(post.Id);
            var result = _context.Posts.FirstOrDefault(p => p.Id == post.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldAddLike_WhenLikeDoesNotExist()
        {
            // Arrange
            var post = new Post
            {
                TopicId = 1,
                Content = "Test",
                UserId = 1
            };
            var userId = 1;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            await _postService.ToggleLikeAsync(post.Id, userId);
            var result = _context.Likes.FirstOrDefault(l => l.PostId == post.Id && l.UserId == userId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldRemoveLike_WhenLikeExists()
        {
            // Arrange
            var userId = 1;
            var postId = 5;
            var post = new Post
            {
                Id = postId,
                TopicId = 1,
                Content = "Test",
                UserId = 1,
                Likes = new List<Like> { new Like { PostId = postId, UserId = userId } }
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Act
            await _postService.ToggleLikeAsync(postId, userId);
            var result = _context.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

            // Assert
            Assert.Null(result);
        }
    }
}
