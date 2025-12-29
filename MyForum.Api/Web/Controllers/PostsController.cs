using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Application.Extensions;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Core.DTOs.Requests;
using FluentValidation;

namespace MyForum.Api.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;
        private readonly IValidator<CreatePostRequest> _createPostRequestValidator;
        public PostsController(ILogger<PostsController> logger, IPostService postService, IValidator<CreatePostRequest> createPostRequestValidator)
        {
            _postService = postService;
            _logger = logger;
            _createPostRequestValidator = createPostRequestValidator;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreatePostRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _createPostRequestValidator.ValidateAsync(request, cancellationToken);
            
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

                await _postService.CreateAsync(
                    threadId: request.ThreadId,
                    content: request.Content,
                    authorName: request.AuthorName,
                    postPassword: request.PostPassword ?? string.Empty,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    files: request.Files,
                    cancellationToken: cancellationToken);

                return Ok(new { success = true, message = "Пост создан" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Неверная операция при добавлении комментария.");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении комментария.");
                return StatusCode(500, "Произошла ошибка при добавлении комментария.");
            }
        }
    }
}