using MyForum.Core.DTOs;

namespace MyForum.Core.Interfaces.Services
{
    public interface IThreadService
    {
        Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default);
        Task<int> CreateThreadWithPostAsync(int boardId, string subject, string postContent,
            string authorName, string postPassword, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default);
    }
}