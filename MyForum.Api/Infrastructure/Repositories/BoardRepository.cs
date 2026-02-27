using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Api.Infrastructure.Repositories
{
    public class BoardRepository : Repository<Board>, IBoardRepository
    {
        public BoardRepository(ForumDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Board>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Boards
                .OrderBy(b => b.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Board>> GetAllIncludingHiddenAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Boards
                .IgnoreQueryFilters()
                .OrderBy(b => b.Position)
                .ToListAsync(cancellationToken);
        }

        public async Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, int threadLimit, CancellationToken cancellationToken = default)
        {
            if (threadLimit < 1)
                threadLimit = 1;

            const int maxThreadLimit = 200;
            if (threadLimit > maxThreadLimit)
                threadLimit = maxThreadLimit;

            var board = await _context.Boards
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ShortName == boardShortName, cancellationToken);

            if (board == null)
                return null;

            var threads = await _context.Threads
                .AsNoTracking()
                .Where(t => t.BoardId == board.Id)
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.LastBumpAt)
                .Take(threadLimit)
                .ToListAsync(cancellationToken);

            var threadIds = threads.Select(t => t.Id).ToList();

            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => threadIds.Contains(p.ThreadId))
                .Include(p => p.Files)
                .ToListAsync(cancellationToken);

            foreach (var thread in threads)
            {
                var originalPost = posts
                    .FirstOrDefault(p => p.ThreadId == thread.Id && p.IsOriginal);

                var lastThreePosts = posts
                    .Where(p => p.ThreadId == thread.Id && !p.IsOriginal)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .OrderBy(p => p.CreatedAt)
                    .ToList();

                thread.Posts = originalPost != null
                    ? new[] { originalPost }.Concat(lastThreePosts).ToList()
                    : lastThreePosts;
            }

            board.Threads = threads;
            return board;
        }

        public async Task<Board?> GetByIdIncludingHiddenAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Boards
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public Task<Board?> GetByShortNameAsync(string shortName, CancellationToken cancellationToken = default)
        {
            return _context.Boards
                .FirstOrDefaultAsync(b => b.ShortName == shortName, cancellationToken);
        }
    }
}