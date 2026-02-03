using MyForum.Api.Core.DTOs.Common;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IThreadRepository : IRepository<Thread>
    {
        Task<Thread?> GetThreadWithOriginalPostAsync(string boardShortName, int threadId, CancellationToken ct = default);
        Task<Thread?> GetThreadWithPostsByIdAsync(string boardShortName, int id, int postLimit, CancellationToken cancellationToken = default);
        Task<PagedResult<Thread>> GetPagedThreadsByBoardShortNameAsync(string boardShortName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<PagedResult<Thread>> GetPagedThreadsByBoardWithPostsAsync(int boardId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<List<Thread>> GetThreadsByCursorAsync(string boardShortName, DateTime? cursor, int limit, CancellationToken cancellationToken = default);
    }
}