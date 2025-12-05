using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IBoardRepository : IRepository<Board>
    {
        Task<Board?> GetByShortNameAsync(string shortName, CancellationToken cancellationToken = default);
        Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default);
    }
}