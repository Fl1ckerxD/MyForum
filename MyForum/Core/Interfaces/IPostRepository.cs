using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<Post> GetWithLikesAsync(int id);
        Task AddAsync(int topicId, string content, int userId);
    }
}
