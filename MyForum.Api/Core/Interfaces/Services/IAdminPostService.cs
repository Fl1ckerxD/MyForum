using MyForum.Api.Core.DTOs;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IAdminPostService
    {
        Task<IReadOnlyList<AdminPostDto>> GetByThreadAsync(
            int threadId,
            int limit = 50,
            int? afterId = null,
            string? search = null,
            bool? isDeleted = null,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(int postId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int postId, CancellationToken cancellationToken = default);
        Task RestoreAsync(int postId, CancellationToken cancellationToken = default);
    }
}