using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Application.Extensions;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Core.DTOs.Requests;
using FluentValidation;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;

namespace MyForum.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadsController : ControllerBase
    {
        private readonly ILogger<ThreadsController> _logger;
        private readonly IThreadService _threadService;
        private readonly IValidator<CreateThreadRequest> _createThreadRequestValidator;

        public ThreadsController(ILogger<ThreadsController> logger, IThreadService threadService, IValidator<CreateThreadRequest> createThreadRequestValidator)
        {
            _logger = logger;
            _threadService = threadService;
            _createThreadRequestValidator = createThreadRequestValidator;
        }

        [HttpGet("{boardShortName}/{threadId}")]
        public async Task<ActionResult<ThreadDto>> GetThread(string boardShortName, int threadId, CancellationToken cancellationToken)
        {
            try
            {
                var thread = await _threadService.GetThreadWithPostsById(boardShortName, threadId, cancellationToken);

                if (thread == null)
                {
                    _logger.LogWarning("Тред с ID '{ThreadId}' не найден.", threadId);
                    return NotFound();
                }

                return Ok(thread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке страницы.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Произошла ошибка при обработке запроса."));
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CreateThreadResponse>> Create([FromForm] CreateThreadRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _createThreadRequestValidator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var ipAddress = HttpContext.GetClientIp();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var threadId = await _threadService.CreateThreadWithPostAsync(
                    boardId: request.BoardId,
                    subject: request.Subject,
                    postContent: request.OriginalPost.Content,
                    authorName: request.OriginalPost.AuthorName,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    files: request.OriginalPost.Files,
                    cancellationToken: cancellationToken);

                return Created($"/api/{request.BoardShortName}/{threadId}",
                    new CreateThreadResponse
                    {
                        ThreadId = threadId,
                        BoardShortName = request.BoardShortName,
                        Message = "Тред создан"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании треда.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Ошибка при создании треда."));
            }
        }
    }
}
