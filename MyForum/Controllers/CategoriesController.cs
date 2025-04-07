using Microsoft.AspNetCore.Mvc;
using MyForum.Services.CategoryServices;

namespace MyForum.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;
        public CategoriesController(ILogger<CategoriesController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                ModelState.AddModelError(nameof(categoryName), "Имя категории не может быть пустым.");
                return BadRequest(ModelState);
            }

            try
            {
                var category = await _categoryService.GetCategoryByNameAsync(categoryName);

                if (category == null)
                {
                    _logger.LogWarning($"Категория с именем '{categoryName}' не найдена.");
                    return NotFound(); // Возвращаем 404, если категория не найдена
                }

                return View(category); // Передаем категорию в представление
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении категории '{categoryName}'.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
