using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ForumContext _context;
        private bool _disposed;
        public ICategoryRepository _categories;
        public ILikeRepository _likes;
        public IPostRepository _posts;
        public ITopicRepository _topics;
        public IUserRepository _users;

        public UnitOfWork(ForumContext context)
        {
            _context = context;
        }

        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public ILikeRepository Likes => _likes ??= new LikeRepository(_context);
        public IPostRepository Posts => _posts ??= new PostRepository(_context);
        public ITopicRepository Topics => _topics ??= new TopicRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
