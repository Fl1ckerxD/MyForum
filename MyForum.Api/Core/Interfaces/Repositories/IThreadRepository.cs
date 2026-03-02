using MyForum.Api.Core.DTOs;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IThreadRepository : IRepository<Thread>
    {
        Task<Thread?> GetThreadWithOriginalPostAsync(string boardShortName, int threadId, CancellationToken ct = default);
        Task<Thread?> GetThreadWithPostsByIdAsync(string boardShortName, int id, int postLimit, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Thread>> GetThreadsByCursorWithPostsAsync(string boardShortName, DateTime? cursor, int limit, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Thread>> GetThreadsAsync(
            int limit,
            DateTime? cursor,
            string? search = null,
            string? board = null,
            bool? isDeleted = null,
            bool? isLocked = null,
            CancellationToken cancellationToken = default);
        Task<Thread?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken = default);
        Task<ThreadStats> RecountThreadStatsAsync(int id, CancellationToken cancellationToken = default);
    }
}