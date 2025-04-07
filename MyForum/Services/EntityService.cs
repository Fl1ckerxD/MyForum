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
        public async Task<IActionResult> DeleteEntityAsync<TEntity>(int entityId, Func<ForumContext, DbSet<TEntity>> getDbSet) where TEntity : class
        {
            var entity = await getDbSet(_context)
                .Include(e => EF.Property<User>(e, "User"))
                .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == entityId);

            if (entity == null)
                return new NotFoundResult();

            getDbSet(_context).Remove(entity);
            await _context.SaveChangesAsync();

            return new OkResult();
        }
    }
}
