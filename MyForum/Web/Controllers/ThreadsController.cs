using Microsoft.AspNetCore.Mvc;
using MyForum.Application.Extensions;
using MyForum.Core.Interfaces.Services;
using MyForum.Core.DTOs.Requests;
using FluentValidation;

namespace MyForum.Web.Controllers
{
    public class ThreadsController : Controller
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

        public async Task<IActionResult> Index(string boardShortName, int threadId, CancellationToken cancellationToken)
        {
            try
            {
                var thread = await _threadService.GetThreadWithPostsById(boardShortName, threadId, cancellationToken);

                if (thread == null)
                {
                    _logger.LogWarning("Тред с ID '{ThreadId}' не найден.", threadId);
                    return NotFound();
                }

                return View(thread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке страницы.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateThreadRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _createThreadRequestValidator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return BadRequest(ModelState);
            }

            try
            {
                var ipAddress = HttpContext.GetClientIp();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var threadId = await _threadService.CreateThreadWithPostAsync(
                    boardId: request.BoardId,
                    subject: request.Subject,
                    postContent: request.OriginalPost.Content,
                    authorName: request.OriginalPost.AuthorName,
                    postPassword: request.OriginalPost.PostPassword ?? string.Empty,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    cancellationToken: cancellationToken);

                return RedirectToAction("Index", "Threads", new { request.BoardShortName, threadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании треда.");
                return StatusCode(500, "Ошибка при создании треда.");
            }
        }
    }
}
