using Microsoft.EntityFrameworkCore;
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

        public async Task<Post?> GetByIdIncludingDeletedAsync(int postId, CancellationToken cancellationToken = default)
        {
            return await _context.Posts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
        }

        public async Task<IReadOnlyList<Post>> GetByThreadIncludingDeletedAsync(
            int threadId,
            int limit = 50,
            int? afterId = null,
            string? search = null,
            bool? isDeleted = null,
            CancellationToken cancellationToken = default)
        {
            if (limit < 1)
                limit = 1;

            const int maxLimit = 200;
            if (limit > maxLimit)
                limit = maxLimit;

            var query = _context.Posts
                .IgnoreQueryFilters()
                .Where(p => p.ThreadId == threadId);

            if (afterId.HasValue)
                query = query.Where(p => p.Id > afterId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => EF.Functions.ILike(p.Content, $"%{search}%"));

            if (isDeleted.HasValue)
                query = query.Where(p => p.IsDeleted == isDeleted.Value);

            return await query
                .OrderBy(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Post>> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default)
        {
            if (limit < 1)
                limit = 1;

            const int maxLimit = 200;
            if (limit > maxLimit)
                limit = maxLimit;

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