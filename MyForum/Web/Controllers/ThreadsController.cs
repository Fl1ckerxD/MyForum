using Microsoft.AspNetCore.Mvc;
using MyForum.Core.DTOs;
using MyForum.Core.Interfaces.Services;

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
        public async Task<IActionResult> Create(CreateThreadDto createThreadDto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(createThreadDto.Subject))
                ModelState.AddModelError(nameof(createThreadDto.Subject), "Введите название трэда.");
            else if (createThreadDto.Subject.Length > 100)
                ModelState.AddModelError(nameof(createThreadDto.Subject), "Длина не должна превышать больше 100 символов.");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var threadId = await _threadService.CreateThreadAsync(createThreadDto.BoardId, createThreadDto.Subject, cancellationToken);

                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                await _postService.CreateAsync(
                    threadId: threadId,
                    content: createThreadDto.OriginalPost.Content,
                    authorName: createThreadDto.OriginalPost.AuthorName,
                    postPassword: createThreadDto.OriginalPost.PostPassword ?? string.Empty,
                    isOriginalPost: true,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    cancellationToken: cancellationToken);

                return RedirectToAction("Index", "Threads", new { createThreadDto.BoardShortName, threadId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании треда.");
                return StatusCode(500, "Ошибка при создании треда.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int topicId)
        {
            try
            {
                //await _uow.Topics.DeleteAsync(topicId);
                //await _uow.SaveAsync();

                //_logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) удалил топик {topicId}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении топика");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        private string GetClientIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }

            if (Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                return realIp.FirstOrDefault();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
