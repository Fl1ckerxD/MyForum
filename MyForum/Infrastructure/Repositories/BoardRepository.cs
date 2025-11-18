using Microsoft.EntityFrameworkCore;
using MyForum.Core.DTOs;
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

        public async Task<IEnumerable<BoardNamesDto>> GetAllNamesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Boards.Select(b => new BoardNamesDto(b.Name, b.ShortName)).ToListAsync(cancellationToken);
        }
    }
}