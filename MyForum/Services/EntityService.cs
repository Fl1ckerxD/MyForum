using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Services
{
    public class EntityService : IEntityService
    {
        private readonly ForumContext _context;
        public EntityService(ForumContext context)
        {
            _context = context;
        }
        public async Task DeleteEntityAsync<TEntity>(int entityId, Func<ForumContext, DbSet<TEntity>> getDbSet) where TEntity : class
        {
            var entity = await getDbSet(_context)
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == entityId);

            if (entity == null)
                throw new ArgumentException("Объект не найден.");

            getDbSet(_context).Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
