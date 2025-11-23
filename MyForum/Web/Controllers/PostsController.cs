using Microsoft.AspNetCore.Mvc;
using MyForum.Application.Extensions;
using MyForum.Core.Interfaces.Services;
using MyForum.Web.Requests;

namespace MyForum.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;
        public PostsController(ILogger<PostsController> logger, IPostService postService)
        {
            _postService = postService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Комментарий не может быть пустым." });

            else if (request.Content.Length > 15000)
                return BadRequest(new { message = "Длина комментария не должна превышать 15000 символов." });

            try
            {
                var ipAddress = HttpContext.GetClientIp();
                var userAgent = Request.Headers["User-Agent"].ToString();

                await _postService.CreateAsync(
                    threadId: request.ThreadId,
                    content: request.Content,
                    authorName: request.AuthorName,
                    postPassword: request.PostPassword ?? string.Empty,
                    isOriginalPost: false,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    cancellationToken: cancellationToken);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении комментария.");
                return StatusCode(500, "Произошла ошибка при добавлении комментария.");
            }
        }
    }
}