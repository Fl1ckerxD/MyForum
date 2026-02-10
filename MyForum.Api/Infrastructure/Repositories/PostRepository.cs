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

        public async Task<Post?> GetByIdIncludingDeletedAsync(int postId, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
        }

        public async Task<IReadOnlyList<Post>> GetByThreadIncludingDeletedAsync(int threadId, CancellationToken cancellationToken)
        {
            return await _context.Posts
                .IgnoreQueryFilters()
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Post>> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default)
        {
            var query = _context.Posts
                .AsNoTracking()
                .Where(p => p.ThreadId == threadId && p.Id > afterId)
                .OrderBy(p => p.CreatedAt)
                .Include(p => p.Files)
                .Take(limit);

            return query.ToListAsync(cancellationToken);
        }
    }
}