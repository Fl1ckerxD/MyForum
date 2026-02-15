using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Api.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ForumDbContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(ForumDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var enetity = await GetByIdAsync(id, cancellationToken);
            if (enetity != null) _dbSet.Remove(enetity);
            else throw new ArgumentException("Объект не найден.");
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                e => EF.Property<int>(e, "Id") == id,
                cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(id, cancellationToken);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
