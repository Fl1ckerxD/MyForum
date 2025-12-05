using Microsoft.EntityFrameworkCore;
using MyForum.Core.DTOs.Common;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;
using Thread = MyForum.Core.Entities.Thread;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;

namespace MyForum.Infrastructure.Repositories
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

        public async Task<Thread?> GetThreadWithPostsByIdAsync(string boardShortName, int id, CancellationToken cancellationToken = default)
        {
            return await _context.Threads
                .Include(t => t.Posts)
                .Include(b => b.Board)
                .FirstOrDefaultAsync(t => t.Board.ShortName == boardShortName && t.Id == id, cancellationToken);
        }
    }
}