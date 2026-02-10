using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBanRepository Bans { get; }
        IBoardRepository Boards { get; }
        IStaffAccountRepository StaffAccounts { get; }
        IPostRepository Posts { get; }
        IPostFileRepository PostFiles { get; }
        IThreadRepository Threads { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }
}
