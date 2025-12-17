using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Web.Controllers
{
    public class BoardsController : Controller
    {
        private readonly IBoardService _boardService;
        private readonly ILogger<BoardsController> _logger;
        public BoardsController(ILogger<BoardsController> logger, IBoardService boardService)
        {
            _logger = logger;
            _boardService = boardService;
        }

        public async Task<IActionResult> Index(string boardShortName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(boardShortName))
            {
                ModelState.AddModelError(nameof(boardShortName), "Имя категории не может быть пустым.");
                return BadRequest(ModelState);
            }

            try
            {
                var board = await _boardService.GetBoardWithThreadsAndPostsAsync(boardShortName, cancellationToken);

                if (board == null)
                {
                    _logger.LogWarning("Категория с именем '{BoardShortName}' не найдена.", boardShortName);
                    return NotFound();
                }
                _logger.LogInformation("Категория '{BoardShortName}' успешно получена.", boardShortName);
                return View(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении категории '{BoardShortName}'.", boardShortName);
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
