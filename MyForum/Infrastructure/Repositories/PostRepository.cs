using Microsoft.EntityFrameworkCore;
using MyForum.Core.DTOs.Common;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(ForumDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Post>> GetPagedPostsByThreadIdAsync(int threadId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Posts
                .Where(p => p.ThreadId == threadId)
                .Include(p => p.Files)
                .Include(p => p.Replies)
                .OrderBy(p => p.CreatedAt);
            
            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            
            return new PagedResult<Post>(items, totalItems, pageNumber, pageSize);
        }
    }
}