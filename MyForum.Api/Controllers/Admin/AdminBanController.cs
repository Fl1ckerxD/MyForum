using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bans")]
    [Authorize(Roles = "Admin")]
    public class AdminBanController : ControllerBase
    {
        private readonly IBanService _banService;
        private readonly ILogger<AdminBanController> _logger;

        public AdminBanController(IBanService banService, ILogger<AdminBanController> logger)
        {
            _banService = banService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Ban(
            [FromBody] CreateBanRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _banService.BanAsync(
                    request.IpHash,
                    request.BoardId,
                    request.Reason,
                    request.ExpiresAt,
                    cancellationToken);

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Неверные параметры запроса при создании бана: {IpHash}, {BoardId}",
                                request.IpHash, request.BoardId);
                return BadRequest(new { error = "invalid_request", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании бана: {IpHash}, {BoardId}", request.IpHash, request.BoardId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Unban(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                await _banService.UnbanAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Неверный идентификатор бана: {BanId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Бан не найден: {BanId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при разбане: {BanId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }
    }
}