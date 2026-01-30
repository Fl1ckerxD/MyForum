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

        public async Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default)
        {
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
                .Take(20)
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

            board.Threads = threads;
            return board;
        }
        public Task<Board?> GetByShortNameAsync(string shortName, CancellationToken cancellationToken = default)
        {
            return _context.Boards
                .FirstOrDefaultAsync(b => b.ShortName == shortName, cancellationToken);
        }
    }
}