using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _uof;
        public PostService(IUnitOfWork uow)
        {
            _uof = uow;
        }

        public async Task ToggleLikeAsync(int postId, int userId)
        {
            var post = await _uof.Posts.GetWithLikesAsync(postId);

            if (post == null)
                throw new ArgumentException("Пост не найден.");

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
                post.Likes.Remove(existingLike);
            else
            {
                var newLike = new Like
                {
                    PostId = postId,
                    UserId = userId
                };
                post.Likes.Add(newLike);
            }
            await _uof.SaveAsync();
        }
    }
}
