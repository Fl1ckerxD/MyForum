using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Api.Infrastructure.Repositories
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