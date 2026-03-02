using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.Exceptions;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/posts")]
    [Authorize(Roles = "Admin")]
    public class AdminPostsController : ControllerBase
    {
        private readonly IAdminPostService _postService;
        private readonly IBanService _banService;
        private readonly ILogger<AdminPostsController> _logger;
        private readonly IValidator<CreatePostBanRequest> _createPostBanRequestValidator;

        public AdminPostsController(
            IAdminPostService postService,
            ILogger<AdminPostsController> logger,
            IBanService banService,
            IValidator<CreatePostBanRequest> createPostBanRequestValidator)
        {
            _postService = postService;
            _logger = logger;
            _banService = banService;
            _createPostBanRequestValidator = createPostBanRequestValidator;
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

        [HttpPost("{id:int}/ban")]
        public async Task<IActionResult> BanPost(
            int id,
            [FromBody] CreatePostBanRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = _createPostBanRequestValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Ошибка валидации",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            try
            {
                await _banService.BanAsync(
                    id,
                    request.BoardId,
                    request.Reason,
                    request.ExpiresAt,
                    cancellationToken);

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UserAlreadyBannedException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при бане автора поста {PostId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }
    }
}