using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class TopicRepository : Repository<Topic>, ITopicRepository
    {
        public TopicRepository(ForumContext context) : base(context)
        {
        }

        public override async Task<Topic?> GetByIdAsync(int id)
        {
            return await _context.Topics
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.Posts).ThenInclude(x => x.User)
                .Include(x => x.Posts).ThenInclude(x => x.Likes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
