using MyForum.Core.DTOs;
using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IBoardRepository : IRepository<Board>
    {
        Task<IEnumerable<BoardNamesDto>> GetAllNamesAsync(CancellationToken cancellationToken = default);
        Task<BoardDto> GetByShortNameAsync(string boardShortName, CancellationToken cancellationToken = default);
    }
}