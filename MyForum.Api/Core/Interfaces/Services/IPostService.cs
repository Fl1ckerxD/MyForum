using MyForum.Api.Core.DTOs.Responses;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IPostService
    {
        Task<CreatePostResponse> CreateAsync(int threadId, string content, string authorName, string ipAddress,
            string userAgent, List<IFormFile>? files = null, int? replyToPostId = null, CancellationToken cancellationToken = default);

        Task<int> CreateAsync(Thread thread, string content, string authorName, string ipAddress,
            string userAgent, List<IFormFile>? files = null, CancellationToken cancellationToken = default);
        Task<GetPostsResponse> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default);
    }
}
