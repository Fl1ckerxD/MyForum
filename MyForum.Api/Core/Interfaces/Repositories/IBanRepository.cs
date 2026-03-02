using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IBanRepository : IRepository<Ban>
    {
        Task<bool> IsBannedAsync(string ipHash, int? boardId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Ban>> GetBansAsync(
            int limit = 50,
            int? beforeId = null,
            string? status = null,
            string? boardShortName = null,
            CancellationToken cancellationToken = default);
    }
}