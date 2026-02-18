using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IBoardRepository : IRepository<Board>
    {
        Task<Board?> GetByIdIncludingHiddenAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Board>> GetAllIncludingHiddenAsync(CancellationToken cancellationToken = default);
        Task<Board?> GetByShortNameAsync(string shortName, CancellationToken cancellationToken = default);
        Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, int threadLimit, CancellationToken cancellationToken = default);
    }
}