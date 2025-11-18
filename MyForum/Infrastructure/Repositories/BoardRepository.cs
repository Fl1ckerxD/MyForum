using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyForum.Core.DTOs;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class BoardRepository : Repository<Board>, IBoardRepository
    {
        private readonly IMapper _mapper;
        public BoardRepository(ForumDbContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<BoardNamesDto>> GetAllNamesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Boards.Select(b => new BoardNamesDto(b.Name, b.ShortName)).ToListAsync(cancellationToken);
        }

        public async Task<BoardDto> GetByShortNameAsync(string boardShortName, CancellationToken cancellationToken = default)
        {
            return await _context.Boards
                .Where(b => b.ShortName == boardShortName)
                .Include(b => b.Threads)
                    .ThenInclude(p => p.Posts)
                .Select(b => _mapper.Map<BoardDto>(b))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}