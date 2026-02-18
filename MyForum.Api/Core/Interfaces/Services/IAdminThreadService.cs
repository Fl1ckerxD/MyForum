using MyForum.Api.Core.DTOs;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IAdminThreadService
    {
        Task<IReadOnlyList<AdminThreadDto>> GetThreadsAsync(
            int limit,
            DateTime? cursor,
            string? search = null,
            string? board = null,
            bool? isDeleted = null,
            bool? isLocked = null,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(int threadId, CancellationToken cancellationToken);
        Task SoftDeleteAsync(int threadId, CancellationToken cancellationToken);
        Task RestoreAsync(int threadId, CancellationToken cancellationToken);
        Task LockAsync(int threadId, CancellationToken cancellationToken);
        Task UnlockAsync(int threadId, CancellationToken cancellationToken);
    }
}