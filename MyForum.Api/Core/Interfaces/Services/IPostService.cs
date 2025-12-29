using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Common;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task CreateAsync(int threadId, string content, string authorName, string postPassword, 
            string ipAddress, string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);

        Task CreateAsync(Thread thread, string content, string authorName, string postPassword, 
            string ipAddress, string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);

        Task<PagedResult<PostDto>> GetPagedPostsByThreadIdAsync(int threadId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}
