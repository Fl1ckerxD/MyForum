using Microsoft.EntityFrameworkCore;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;
using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Infrastructure.Repositories
{
    public class ThreadRepository : Repository<Thread>, IThreadRepository
    {
        public ThreadRepository(ForumDbContext context) : base(context)
        {
        }

        public async Task<Thread?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default)
        {
            return await _context.Threads
                .Include(t => t.Posts)
                .Include(b => b.Board)
                .FirstOrDefaultAsync(t => t.Board.ShortName == boardShortName && t.Id == id, cancellationToken);
        }
    }
}