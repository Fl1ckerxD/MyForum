using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<PagedResult<Post>> GetPagedPostsByThreadIdAsync(int threadId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<List<Post>> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default);
    }
}