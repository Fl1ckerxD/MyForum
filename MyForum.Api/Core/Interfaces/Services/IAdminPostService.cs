using MyForum.Api.Core.DTOs;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IAdminPostService
    {
        Task<IReadOnlyList<AdminPostDto>> GetByThreadAsync(int threadId, CancellationToken cancellationToken);
        Task DeleteAsync(int postId, CancellationToken cancellationToken);
        Task SoftDeleteAsync(int postId, CancellationToken cancellationToken);
        Task RestoreAsync(int postId, CancellationToken cancellationToken);
    }
}