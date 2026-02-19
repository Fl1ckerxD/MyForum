using MyForum.Api.Core.DTOs;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IBanService
    {
        Task<IReadOnlyList<BanDto>> GetBansAsync(
            int limit = 50,
            int? beforeId = null,
            string? status = null,
            string? boardShortName = null,
            CancellationToken cancellationToken = default);
        Task<bool> IsBannedAsync(string ipHash, int? boardId, CancellationToken cancellationToken);
        Task BanAsync(string ipHash, int? boardId, string reason, DateTime? expiresAt, CancellationToken cancellationToken);
        Task BanAsync(int postId, int? boardId, string reason, DateTime? expiresAt, CancellationToken cancellationToken);
        Task UnbanAsync(int banId, CancellationToken cancellationToken);
    }
}