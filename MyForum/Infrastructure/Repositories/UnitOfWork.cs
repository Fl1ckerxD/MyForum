using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;
using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ForumDbContext _context;
        private bool _disposed;
        public IRepository<Ban> _bans;
        public IRepository<Board> _boards;
        public IRepository<BoardModerator> _boardModerators;
        public IRepository<Post> _posts;
        public IRepository<PostFile> _postFiles;
        public IRepository<Thread> _threads;

        public UnitOfWork(ForumDbContext context)
        {
            _context = context;
        }

        public IRepository<Ban> Bans => _bans ??= new Repository<Ban>(_context);
        public IRepository<Board> Boards => _boards ??= new Repository<Board>(_context);
        public IRepository<BoardModerator> BoardModerators => _boardModerators ??= new Repository<BoardModerator>(_context);
        public IRepository<Post> Posts => _posts ??= new Repository<Post>(_context);
        public IRepository<PostFile> PostFiles => _postFiles ??= new Repository<PostFile>(_context);
        public IRepository<Thread> Threads => _threads ??= new Repository<Thread>(_context);

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
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
