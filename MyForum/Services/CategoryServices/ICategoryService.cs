using MyForum.Models;

namespace MyForum.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryByNameAsync(string categoryName);
        Task<ICollection<Category>> GetAllCategoriesAsync();
    }
}
