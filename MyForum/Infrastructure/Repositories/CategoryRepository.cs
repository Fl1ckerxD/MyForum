using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ForumContext context) : base(context)
        {
        }

        public async Task<IEnumerable<string>> GetAllNamesAsync()
        {
            return await _context.Categories.Select(c => c.Name).ToListAsync();
        }

        public async Task<Category> GetCategoryByNameAsync(string categoryName)
        {
            return await _context.Categories
            .Include(x => x.Topics)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(c => c.Name == categoryName);
        }
    }
}
