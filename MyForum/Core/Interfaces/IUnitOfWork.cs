namespace MyForum.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        ILikeRepository Likes { get; }
        IPostRepository Posts { get; }
        ITopicRepository Topics { get; }
        IUserRepository Users { get; }
        Task<int> SaveAsync();
    }
}
