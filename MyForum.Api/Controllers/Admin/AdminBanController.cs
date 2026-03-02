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
    [Route("api/admin/bans")]
    [Authorize(Roles = "Admin")]
    public class AdminBanController : ControllerBase
    {
        private readonly IBanService _banService;
        private readonly ILogger<AdminBanController> _logger;
        private readonly IValidator<CreateBanRequest> _createBanRequestValidator;

        public AdminBanController(IBanService banService, ILogger<AdminBanController> logger, IValidator<CreateBanRequest> createBanRequestValidator)
        {
            _banService = banService;
            _logger = logger;
            _createBanRequestValidator = createBanRequestValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BanDto>>> Get(
            [FromQuery] int limit = 50,
            [FromQuery] int? beforeId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? boardShortName = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bans = await _banService.GetBansAsync(limit, beforeId, status, boardShortName, cancellationToken);
                return Ok(bans);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Неверные параметры фильтра блокировки: {Status}, {BoardShortName}", status, boardShortName);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка банов");
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Ban(
            [FromBody] CreateBanRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = _createBanRequestValidator.Validate(request);
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
                    request.IpHash,
                    request.BoardShortName,
                    request.Reason,
                    request.ExpiresAt,
                    cancellationToken);

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Неверные параметры запроса при создании бана: {IpHash}, {BoardId}", request.IpHash, request.BoardShortName);
                return BadRequest(new { error = "invalid_request", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Неверные параметры запроса при создании бана: {IpHash}, {BoardId}", request.IpHash, request.BoardShortName);
                return BadRequest(new { error = "invalid_request", message = ex.Message });
            }
            catch (UserAlreadyBannedException ex)
            {
                _logger.LogWarning(ex, "Повторная попытка блокировки: {IpHash}, {BoardId}", request.IpHash, request.BoardShortName);
                return Conflict(new { error = "already_banned", message = ex.Message });
            }
            catch (BoardNotFoundException ex)
            {
                _logger.LogWarning(ex, "Неверное имя доски: {boardShortName}", request.BoardShortName);
                return BadRequest(new { error = "invalid_request", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании бана: {IpHash}, {BoardId}", request.IpHash, request.BoardShortName);
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
