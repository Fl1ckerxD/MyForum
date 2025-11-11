using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface ILikeRepository : IRepository<Like>
    {
        Task<int> GetLikesCountAsync(int postId);
    }
}
