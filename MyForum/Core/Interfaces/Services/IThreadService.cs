using MyForum.Core.DTOs;

namespace MyForum.Core.Interfaces.Services
{
    public interface IThreadService
    {
        Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default);
        Task<int> CreateThreadAsync(int boardId, string subject, CancellationToken cancellationToken = default);
    }
}