using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly ForumContext _context;
        public CategoryService(ForumContext context)
        {
            _context = context;
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
