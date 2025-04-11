using MyForum.Models;
using MyForum.Services.CategoryServices;

namespace MyForum.Tests.Services.CategoryServices
{
    public class CategoryServiceTests
    {
        private readonly ForumContext _context;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            var options = DbContext.GetOptions();
            _context = new ForumContext(options);
            _categoryService = new CategoryService(_context);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryName = "Sample";
            var expectedCategory = new Category { Name = categoryName, Description = "" };

            _context.Categories.Add(expectedCategory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _categoryService.GetCategoryByNameAsync(categoryName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCategory.Name, result.Name);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryName = "SampleEe";

            // Act
            var result = await _categoryService.GetCategoryByNameAsync(categoryName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Name = "Category 1", Description = ""},
                new Category { Name = "Category 2", Description = ""}
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categories.Count, result.Count);
            Assert.Equal(categories.Select(c => c.Name), result.Select(c => c.Name));
        }
    }
}
