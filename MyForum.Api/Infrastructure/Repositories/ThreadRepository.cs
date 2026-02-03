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

        public async Task<PagedResult<Thread>> GetPagedThreadsByBoardShortNameAsync(string boardShortName, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Threads
                .Where(t => t.Board.ShortName == boardShortName)
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.LastBumpAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Thread>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<Thread>> GetPagedThreadsByBoardWithPostsAsync(int boardId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Threads
            .Where(t => t.BoardId == boardId)
            .Include(t => t.Posts.OrderBy(p => p.CreatedAt).Take(5))
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.LastBumpAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Thread>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<List<Thread>> GetThreadsByCursorAsync(string boardShortName, DateTime? cursor, int limit, CancellationToken cancellationToken = default)
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