using MyForum.Api.Core.DTOs;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IAdminThreadService
    {
        Task<IReadOnlyList<AdminThreadDto>> GetThreadsAsync(
            int limit,
            DateTime? cursor,
            CancellationToken cancellationToken);

        Task DeleteAsync(int threadId, CancellationToken cancellationToken);
        Task SoftDeleteAsync(int threadId, CancellationToken cancellationToken);
        Task RestoreAsync(int threadId, CancellationToken cancellationToken);
        Task LockAsync(int threadId, CancellationToken cancellationToken);
        Task UnlockAsync(int threadId, CancellationToken cancellationToken);
    }
}