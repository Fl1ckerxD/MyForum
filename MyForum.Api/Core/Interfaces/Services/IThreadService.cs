using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.DTOs.Responses;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IThreadService
    {
        Task<ThreadDto?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default);
        Task<int> CreateThreadWithPostAsync(int boardId, string subject, string postContent,
            string authorName, string ipAddress, string userAgent,
            List<IFormFile>? files = null, CancellationToken cancellationToken = default);
        Task<PagedResult<ThreadDto>> GetThreadsPagedAsync(string boardShortName, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<GetThreadsResponse> GetThreadsByCursorAsync(string boardShortName, DateTime? cursor, int limit = 20, CancellationToken cancellationToken = default);
    }
}