using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces;
using MyForum.Web.ViewModels;

namespace MyForum.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryRepository _categoryRepository;
        public HomeController(ILogger<HomeController> logger, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryRepository.GetAllNamesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка категорий.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
