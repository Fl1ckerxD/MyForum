using MyForum.Api.Core.DTOs.Common;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IThreadRepository : IRepository<Thread>
    {
        Task<Thread?> GetThreadWithPostsByIdAsync(string boardShortName, int id, CancellationToken cancellationToken = default);
        Task<PagedResult<Thread>> GetPagedThreadsByBoardShortNameAsync(string boardShortName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<PagedResult<Thread>> GetPagedThreadsByBoardWithPostsAsync(int boardId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}