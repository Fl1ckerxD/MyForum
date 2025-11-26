using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class PostFileRepository : Repository<PostFile>, IPostFileRepository
    {
        public PostFileRepository(ForumDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PostFile>> GetByPostIdAsync(int postId, CancellationToken cancellationToken = default)
        {
            return await _context.PostFiles
                .Where(pf => pf.PostId == postId)
                .ToListAsync(cancellationToken);
        }
    }
}