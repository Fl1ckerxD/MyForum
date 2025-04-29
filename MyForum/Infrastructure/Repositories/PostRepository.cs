using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(ForumContext context) : base(context)
        {
        }

        public async Task AddAsync(int topicId, string content, int userId)
        {
            var post = new Post
            {
                Content = content,
                TopicId = topicId,
                UserId = userId
            };
            await AddAsync(post);
        }

        public async Task<Post> GetWithLikesAsync(int id)
        {
            return await _context.Posts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
