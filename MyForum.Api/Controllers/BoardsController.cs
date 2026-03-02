using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BoardsController : ControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly IThreadService _threadService;
        private readonly ILogger<BoardsController> _logger;

        public BoardsController(ILogger<BoardsController> logger, IBoardService boardService, IThreadService threadService)
        {
            _logger = logger;
            _boardService = boardService;
            _threadService = threadService;
        }


        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<BoardNamesDto>>> Get()
        {
            try
            {
                var boards = await _boardService.GetAllBoardNamesAsync();
                _logger.LogInformation("Названия досок успешно загружены");
                return Ok(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке названий досок");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Ошибка при загрузке названий досок"));
            }
        }

        [HttpGet("{boardShortName}")]
        public async Task<ActionResult<GetBoardResponse>> GetBoard(string boardShortName, CancellationToken cancellationToken, [FromQuery] int threadLimit = 20)
        {
            try
            {
                var response = await _boardService.GetBoardWithThreadsAndPostsAsync(boardShortName, threadLimit, cancellationToken);
                if (response == null)
                {
                    _logger.LogWarning("Категория с именем '{BoardShortName}' не найдена.", boardShortName);
                    return NotFound();
                }
                _logger.LogInformation("Категория '{BoardShortName}' успешно получена.", boardShortName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении категории '{BoardShortName}'.", boardShortName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Произошла ошибка при обработке запроса."));
            }
        }

        [HttpGet("{boardShortName}/threads")]
        public async Task<ActionResult<GetThreadsResponse>> GetThreads(
            CancellationToken cancellationToken,
            string boardShortName,
            [FromQuery] DateTime? cursor,
            [FromQuery] int limit = 20)
        {
            try
            {
                var response = await _threadService.GetThreadsByCursorAsync(boardShortName, cursor, limit, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тем категории '{BoardShortName}'.", boardShortName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Произошла ошибка при обработке запроса."));
            }
        }
    }
}
