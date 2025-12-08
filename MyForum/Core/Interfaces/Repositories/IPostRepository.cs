using MyForum.Core.DTOs.Common;
using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<PagedResult<Post>> GetPagedPostsByThreadIdAsync(int threadId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}