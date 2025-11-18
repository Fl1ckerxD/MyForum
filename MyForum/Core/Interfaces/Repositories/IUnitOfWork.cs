using MyForum.Core.Entities;
using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Ban> Bans { get; }
        IBoardRepository Boards { get; }
        IRepository<BoardModerator> BoardModerators { get; }
        IRepository<Post> Posts { get; }
        IRepository<PostFile> PostFiles { get; }
        IRepository<Thread> Threads { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}
