using Microsoft.AspNetCore.Mvc;
using MyForum.Application.Extensions;
using MyForum.Core.Interfaces.Services;
using MyForum.Web.Requests;

namespace MyForum.Web.Controllers
{
    public class ThreadsController : Controller
    {
        private readonly ILogger<ThreadsController> _logger;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;

        public ThreadsController(ILogger<ThreadsController> logger, IThreadService threadService, IPostService postService)
        {
            _logger = logger;
            _threadService = threadService;
            _postService = postService;
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
        public async Task<IActionResult> Create([FromBody] CreateThreadRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Subject))
                ModelState.AddModelError(nameof(request.Subject), "Введите название трэда.");
            else if (request.Subject.Length > 100)
                ModelState.AddModelError(nameof(request.Subject), "Длина не должна превышать больше 100 символов.");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var threadId = await _threadService.CreateThreadAsync(request.BoardId, request.Subject, cancellationToken);
                var ipAddress = HttpContext.GetClientIp();
                var userAgent = Request.Headers["User-Agent"].ToString();

                await _postService.CreateAsync(
                    threadId: threadId,
                    content: request.OriginalPost.Content,
                    authorName: request.OriginalPost.AuthorName,
                    postPassword: request.OriginalPost.PostPassword ?? string.Empty,
                    isOriginalPost: true,
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
