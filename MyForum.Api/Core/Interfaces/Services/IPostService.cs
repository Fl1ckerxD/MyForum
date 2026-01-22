using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.DTOs.Responses;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task<CreatePostResponse> CreateAsync(int threadId, string content, string authorName, string ipAddress,
            string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);

        Task<int> CreateAsync(Thread thread, string content, string authorName, string ipAddress,
            string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);
        Task<PagedResult<PostDto>> GetPagedPostsByThreadIdAsync(int threadId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}
