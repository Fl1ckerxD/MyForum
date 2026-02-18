using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/posts")]
    [Authorize(Roles = "Admin")]
    public class AdminPostsController : ControllerBase
    {
        private readonly IAdminPostService _postService;
        private readonly ILogger<AdminPostsController> _logger;

        public AdminPostsController(IAdminPostService postService, ILogger<AdminPostsController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        [HttpGet("thread/{threadId:int}")]
        public async Task<ActionResult<IReadOnlyList<AdminPostDto>>> GetByThread(
            int threadId,
            [FromQuery] int limit = 50,
            [FromQuery] int? afterId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? isDeleted = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var posts = await _postService.GetByThreadAsync(
                    threadId,
                    limit,
                    afterId,
                    search,
                    isDeleted,
                    cancellationToken);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении постов для темы с ID: {ThreadId}", threadId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _postService.SoftDeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка удаления несуществующего поста с ID: {PostId}", id);
                return NotFound(new { message = "Пост не найден" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении поста с ID: {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("{id:int}/restore")]
        public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _postService.RestoreAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка восстановления несуществующего поста с ID: {PostId}", id);
                return NotFound(new { message = "Пост не найден" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при восстановлении поста с ID: {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _postService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Попытка удаления несуществующего поста с ID: {PostId}", id);
                return NotFound(new { message = "Пост не найден" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении поста с ID: {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }
    }
}