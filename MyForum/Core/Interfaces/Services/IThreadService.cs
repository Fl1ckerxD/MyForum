using MyForum.Core.DTOs;
using MyForum.Core.DTOs.Common;

namespace MyForum.Core.Interfaces.Services
{
    public interface IThreadService
    {
        Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default);
        Task<int> CreateThreadWithPostAsync(int boardId, string subject, string postContent,
            string authorName, string postPassword, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default);
        Task<PagedResult<ThreadDto>> GetThreadsPagedAsync(string boardShortName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}