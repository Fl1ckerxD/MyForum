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
            return await _context.Boards
                .Include(b => b.Threads)
                    .ThenInclude(t => t.Posts)
                        .ThenInclude(p => p.Files)
                .FirstOrDefaultAsync(b => b.ShortName == boardShortName, cancellationToken);
        }

        public Task<Board?> GetByShortNameAsync(string shortName, CancellationToken cancellationToken = default)
        {
            return _context.Boards
                .FirstOrDefaultAsync(b => b.ShortName == shortName, cancellationToken);
        }
    }
}