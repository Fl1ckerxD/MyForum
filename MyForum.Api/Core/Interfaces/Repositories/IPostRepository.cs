using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<Post?> GetByIdIncludingDeletedAsync(int postId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Post>> GetByThreadIncludingDeletedAsync(int threadId, CancellationToken cancellationToken);
        Task<List<Post>> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default);
    }
}