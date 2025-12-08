using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Repositories;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ForumDbContext _context;
        private bool _disposed;
        public IRepository<Ban> _bans;
        public IBoardRepository _boards;
        public IRepository<BoardModerator> _boardModerators;
        public IPostRepository _posts;
        public IPostFileRepository _postFiles;
        public IThreadRepository _threads;

        public UnitOfWork(ForumDbContext context)
        {
            _context = context;
        }

        public IRepository<Ban> Bans => _bans ??= new Repository<Ban>(_context);
        public IBoardRepository Boards => _boards ??= new BoardRepository(_context);
        public IRepository<BoardModerator> BoardModerators => _boardModerators ??= new Repository<BoardModerator>(_context);
        public IPostRepository Posts => _posts ??= new PostRepository(_context);
        public IPostFileRepository PostFiles => _postFiles ??= new PostFileRepository(_context);
        public IThreadRepository Threads => _threads ??= new ThreadRepository(_context);

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
