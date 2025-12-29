using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IBoardRepository : IRepository<Board>
    {
        Task<Board?> GetByShortNameAsync(string shortName, CancellationToken cancellationToken = default);
        Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default);
    }
}