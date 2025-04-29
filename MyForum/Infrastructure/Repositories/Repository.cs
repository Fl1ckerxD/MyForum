using Microsoft.EntityFrameworkCore;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ForumContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(ForumContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var enetity = await GetByIdAsync(id);
            if (enetity != null) _dbSet.Remove(enetity);
            else throw new ArgumentException("Объект не найден.");
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
