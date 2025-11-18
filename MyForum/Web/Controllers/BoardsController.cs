using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces.Repositories;

namespace MyForum.Web.Controllers
{
    public class BoardsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BoardsController> _logger;
        public BoardsController(ILogger<BoardsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string boardShortName)
        {
            if (string.IsNullOrWhiteSpace(boardShortName))
            {
                ModelState.AddModelError(nameof(boardShortName), "Имя категории не может быть пустым.");
                return BadRequest(ModelState);
            }

            try
            {
                var board = await _unitOfWork.Boards.GetByShortNameAsync(boardShortName);

                if (board == null)
                {
                    _logger.LogWarning($"Категория с именем '{boardShortName}' не найдена.");
                    return NotFound();
                }

                return View(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении категории '{boardShortName}'.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
