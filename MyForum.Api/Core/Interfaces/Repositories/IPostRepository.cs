using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<Post?> GetByIdIncludingDeletedAsync(int postId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Post>> GetByThreadIncludingDeletedAsync(
            int threadId,
            int limit = 50,
            int? afterId = null,
            string? search = null,
            bool? isDeleted = null,
            CancellationToken cancellationToken = default);
        Task<List<Post>> GetPostsAfterIdAsync(int threadId, int afterId, int limit = 20, CancellationToken cancellationToken = default);
    }
}