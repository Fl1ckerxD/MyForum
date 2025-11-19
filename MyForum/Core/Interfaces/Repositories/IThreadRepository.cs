using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IThreadRepository : IRepository<Thread>
    {
        Task<Thread?> GetThreadWithPostsById(string boardShortName, int id, CancellationToken cancellationToken = default);
    }
}