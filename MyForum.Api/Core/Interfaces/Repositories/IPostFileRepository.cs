using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IPostFileRepository : IRepository<PostFile>
    {
        Task<IEnumerable<PostFile>> GetByPostIdAsync(int postId, CancellationToken cancellationToken = default);
    }
}