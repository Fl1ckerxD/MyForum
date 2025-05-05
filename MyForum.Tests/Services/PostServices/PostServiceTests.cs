using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Repositories;
using MyForum.Infrastructure.Services.PostServices;

namespace MyForum.Tests.Services.PostServices
{
    public class PostServiceTests
    {
        private readonly IUnitOfWork _uow;
        private readonly PostService _postService;
        public PostServiceTests()
        {
            var options = DbContext.GetOptions();
            _uow = new UnitOfWork(new ForumContext(options));

            _postService = new PostService(_uow);
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

            await _uow.Posts.AddAsync(post);
            await _uow.SaveAsync();

            // Act
            await _postService.ToggleLikeAsync(post.Id, userId);
            var like = post.Likes.FirstOrDefault();

            // Assert
            Assert.NotNull(like);
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

            await _uow.Posts.AddAsync(post);
            await _uow.SaveAsync();

            // Act
            await _postService.ToggleLikeAsync(postId, userId);
            var like = post.Likes.FirstOrDefault();

            // Assert
            Assert.Null(like);
        }
    }
}
