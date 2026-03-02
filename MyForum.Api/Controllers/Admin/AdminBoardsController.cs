using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/boards")]
    [Authorize(Roles = "Admin")]
    public class AdminBoardsController : ControllerBase
    {
        private readonly ILogger<AdminBoardsController> _logger;
        private readonly IAdminBoardService _boardService;
        private readonly IValidator<CreateBoardRequest> _createBoardRequestValidator;
        private readonly IValidator<UpdateBoardRequest> _updateBoardRequestValidator;

        public AdminBoardsController(ILogger<AdminBoardsController> logger, IAdminBoardService boardService,
            IValidator<CreateBoardRequest> createBoardRequestValidator,
            IValidator<UpdateBoardRequest> updateBoardRequestValidator)
        {
            _logger = logger;
            _boardService = boardService;
            _createBoardRequestValidator = createBoardRequestValidator;
            _updateBoardRequestValidator = updateBoardRequestValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<BoardDto>>> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var boards = await _boardService.GetAllAsync(cancellationToken);
                return Ok(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении досок");
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost]
        public async Task<ActionResult<BoardDto>> Create([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _createBoardRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var createdBoard = await _boardService.CreateAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetAll), new { id = createdBoard.Id }, createdBoard);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Попытка создания доски с существующим именем: {ShortName}", request.ShortName);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании доски {@request}", request);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateBoardRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _updateBoardRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var result = await _boardService.UpdateAsync(id, request, cancellationToken);
                if (!result)
                    return NotFound(new { message = $"Доска с id {id} не найдена" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ошибка валидации при обновлении доски с id {id}", id);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении доски с id {id} {@request}", id, request);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _boardService.DeleteAsync(id, cancellationToken);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении доски с id {id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        [HttpPatch("{id:int}/visibility")]
        public async Task<IActionResult> PatchVisibility(
            int id,
            [FromBody] UpdateBoardVisibilityRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                return BadRequest();

            try
            {
                var result = await _boardService.UpdateVisibilityAsync(
                    id,
                    request.IsHidden,
                    cancellationToken);

                if (!result)
                    return NotFound(new { message = $"Доска с id {id} не найдена" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при изменении видимости доски с id {id}. IsHidden: {IsHidden}",
                    id, request.IsHidden);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Внутренняя ошибка сервера");
            }
        }
    }
}