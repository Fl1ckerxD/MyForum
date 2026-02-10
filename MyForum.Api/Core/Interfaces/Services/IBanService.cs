namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IBanService
    {
        Task<bool> IsBannedAsync(string ipHash, int? boardId, CancellationToken cancellationToken);
        Task BanAsync(string ipHash, int? boardId, string reason, DateTime? expiresAt, CancellationToken cancellationToken);
        Task UnbanAsync(int banId, CancellationToken cancellationToken);
    }
}