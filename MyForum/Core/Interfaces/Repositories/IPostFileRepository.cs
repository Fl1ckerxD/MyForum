using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IPostFileRepository : IRepository<PostFile>
    {
        Task<IEnumerable<PostFile>> GetByPostIdAsync(int postId, CancellationToken cancellationToken = default);
    }
}