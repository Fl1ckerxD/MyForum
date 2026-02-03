using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Api.Infrastructure.Repositories
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

        public Task<List<Post>> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default)
        {
            var query =  _context.Posts
                .AsNoTracking()
                .Where(p => p.ThreadId == threadId && p.Id > afterId)
                .OrderBy(p => p.CreatedAt)
                .Include(p => p.Files)
                .Take(limit);
            
            return query.ToListAsync(cancellationToken);
        }
    }
}