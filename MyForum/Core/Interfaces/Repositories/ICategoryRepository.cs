using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category> GetCategoryByNameAsync(string categoryName);
        Task<IEnumerable<string>> GetAllNamesAsync();
    }
}
