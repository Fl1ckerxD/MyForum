using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces;

namespace MyForum.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoriesController> _logger;
        public CategoriesController(ILogger<CategoriesController> logger, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
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
                var category = await _categoryRepository.GetCategoryByNameAsync(categoryName);

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
