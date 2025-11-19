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

        public async Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default)
        {
            return await _context.Boards
                .Where(b => b.ShortName == boardShortName)
                .Include(b => b.Threads)
                    .ThenInclude(p => p.Posts)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}