using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/threads")]
    [Authorize(Roles = "Admin")]
    public class AdminThreadsController : ControllerBase
    {
        private readonly IAdminThreadService _threadService;
        private readonly ILogger<AdminThreadsController> _logger;

        public AdminThreadsController(IAdminThreadService service, ILogger<AdminThreadsController> logger)
        {
            _threadService = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminThreadDto>>> GetThreads(
            [FromQuery] int limit = 50,
            [FromQuery] DateTime? cursor = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var threads = await _threadService.GetThreadsAsync(
                    limit,
                    cursor,
                    cancellationToken);

                return Ok(threads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тем");
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _threadService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка удаления несуществующей темы с ID: {ThreadId}", id);
                return NotFound(new { message = "Тема не найдена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении темы с ID: {ThreadId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _threadService.SoftDeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка мягкого удаления несуществующей темы с ID: {ThreadId}", id);
                return NotFound(new { message = "Тема не найдена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при мягком удалении темы с ID: {ThreadId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _threadService.RestoreAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка восстановления несуществующей темы с ID: {ThreadId}", id);
                return NotFound(new { message = "Тема не найдена" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Попытка восстановления темы с ID: {ThreadId}, которая не была удалена", id);
                return BadRequest(new { message = "Тема не была удалена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при восстановлении темы с ID: {ThreadId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("{id:int}/lock")]
        public async Task<IActionResult> Lock(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _threadService.LockAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка блокировки несуществующей темы с ID: {ThreadId}", id);
                return NotFound(new { message = "Тема не найдена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при блокировке темы с ID: {ThreadId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("{id:int}/unlock")]
        public async Task<IActionResult> Unlock(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _threadService.UnlockAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка разблокировки несуществующей темы с ID: {ThreadId}", id);
                return NotFound(new { message = "Тема не найдена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при разблокировке темы с ID: {ThreadId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }
    }
}