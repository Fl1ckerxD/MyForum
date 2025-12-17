using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces.Services;
using MyForum.Web.ViewModels;

namespace MyForum.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBoardService _boardService;
        public HomeController(ILogger<HomeController> logger, IBoardService boardService)
        {
            _logger = logger;
            _boardService = boardService;
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var boards = await _boardService.GetAllBoardNamesAsync();
                _logger.LogInformation("Названия досок успешно загружены");
                return View(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке названий досок");
                return StatusCode(500, "Ошибка при загрузке названий досок");
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
