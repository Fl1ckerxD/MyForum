using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Services.PostServices
{
    public class PostService : IPostService
    {
        private readonly ForumContext _context;
        private readonly IEntityService _entityService;
        public PostService(ForumContext context, IEntityService entityService)
        {
            _context = context;
            _entityService = entityService;
        }

        public async Task AddCommentAsync(int topicId, string content, int userId)
        {
            var post = new Post
            {
                Content = content,
                TopicId = topicId,
                UserId = userId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int postId)
        {
            await _entityService.DeleteEntityAsync(postId, context => context.Posts);
        }

        public async Task ToggleLikeAsync(int postId, int userId)
        {
            var post = await _context.Posts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new ArgumentException("Пост не найден.");

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
                _context.Likes.Remove(existingLike);
            else
            {
                var newLike = new Like
                {
                    PostId = postId,
                    UserId = userId
                };
                _context.Likes.Add(newLike);
            }
            await _context.SaveChangesAsync();
        }
    }
}
