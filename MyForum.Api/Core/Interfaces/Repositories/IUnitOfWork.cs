using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Ban> Bans { get; }
        IBoardRepository Boards { get; }
        IRepository<BoardModerator> BoardModerators { get; }
        IPostRepository Posts { get; }
        IPostFileRepository PostFiles { get; }
        IThreadRepository Threads { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}
