using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Ban> Bans { get; }
        IBoardRepository Boards { get; }
        IRepository<BoardModerator> BoardModerators { get; }
        IRepository<Post> Posts { get; }
        IPostFileRepository PostFiles { get; }
        IThreadRepository Threads { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}
