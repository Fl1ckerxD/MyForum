using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces
{
    public interface ILikeRepository : IRepository<Like>
    {
        Task<int> GetLikesCountAsync(int postId);
    }
}
