using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IBoardRepository : IRepository<Board>
    {
        Task<Board?> GetBoardWithThreadsAndPostsAsync(string boardShortName, CancellationToken cancellationToken = default);
    }
}