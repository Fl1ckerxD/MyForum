using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Infrastructure.Repositories
{
    public class ThreadRepository : Repository<Thread>, IThreadRepository
    {
        public ThreadRepository(ForumDbContext context) : base(context)
        {
        }

        public async Task<Thread?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Threads
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Thread>> GetThreadsAsync(int limit, DateTime? cursor, CancellationToken cancellationToken)
        {
            var query = _context.Threads
                .AsNoTracking()
                .Include(t => t.Board)
                .AsQueryable();

            if (cursor.HasValue)
            {
                query = query.Where(t => t.LastBumpAt < cursor.Value);
            }

            return await query
                .OrderByDescending(t => t.LastBumpAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Thread>> GetThreadsByCursorWithPostsAsync(string boardShortName, DateTime? cursor, int limit, CancellationToken cancellationToken = default)
        {
            var threadsQuery = _context.Threads
                .AsNoTracking()
                .Where(t => t.Board.ShortName == boardShortName);

            if (cursor.HasValue)
            {
                threadsQuery = threadsQuery
                    .Where(t => t.LastBumpAt < cursor.Value);
            }

            threadsQuery = threadsQuery
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.LastBumpAt)
                .Take(limit);

            var threads = await threadsQuery
                .Include(t => t.Board)
                .ToListAsync(cancellationToken);

            var threadIds = threads.Select(t => t.Id).ToList();

            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => threadIds.Contains(p.ThreadId))
                .Include(p => p.Files)
                .ToListAsync(cancellationToken);

            foreach (var thread in threads)
            {
                thread.Posts = posts
                    .Where(p => p.ThreadId == thread.Id)
                    .OrderByDescending(p => p.IsOriginal)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToList();
            }

            return threads;
        }

        public async Task<Thread?> GetThreadWithOriginalPostAsync(string boardShortName, int threadId, CancellationToken cancellationToken = default)
        {
            return await _context.Threads
                .AsNoTracking()
                .Include(t => t.Board)
                .Where(t => t.Id == threadId && t.Board.ShortName == boardShortName)
                .Include(t => t.Posts.Where(p => p.IsOriginal))
                    .ThenInclude(p => p.Files)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Thread?> GetThreadWithPostsByIdAsync(string boardShortName, int id, int postLimit, CancellationToken cancellationToken = default)
        {
            if (postLimit < 1)
                postLimit = 1;

            const int maxPostLimit = 200;
            if (postLimit > maxPostLimit)
                postLimit = maxPostLimit;

            var thread = await _context.Threads
                .AsNoTracking()
                .Include(t => t.Board)
                .FirstOrDefaultAsync(
                    t => t.Board.ShortName == boardShortName && t.Id == id,
                    cancellationToken
                );

            if (thread == null)
                return null;

            var posts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.Files)
                .Where(p => p.ThreadId == thread.Id)
                .OrderByDescending(p => p.IsOriginal)
                .ThenBy(p => p.CreatedAt)
                .Take(postLimit)
                .ToListAsync(cancellationToken);

            thread.Posts = posts;

            return thread;
        }
    }
}