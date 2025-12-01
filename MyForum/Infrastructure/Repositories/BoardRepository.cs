using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
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
                    .ThenInclude(p => p.Posts)
                .FirstOrDefaultAsync(b => b.ShortName == boardShortName, cancellationToken);
        }
    }
}