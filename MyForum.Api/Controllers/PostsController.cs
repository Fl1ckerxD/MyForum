using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Application.Extensions;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Core.DTOs.Requests;
using FluentValidation;
using MyForum.Api.Core.DTOs.Responses;

namespace MyForum.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CreatePostResponse>> Create([FromForm] CreatePostRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _createPostRequestValidator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var ipAddress = HttpContext.GetClientIp();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var createdPostRespose = await _postService.CreateAsync(
                    threadId: request.ThreadId,
                    content: request.Content,
                    authorName: request.AuthorName,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    files: request.Files,
                    replyToPostId: request.ReplyToPostId,
                    cancellationToken: cancellationToken);

                return Created($"/api/posts/{createdPostRespose.Id}", createdPostRespose);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Неверная операция при добавлении поста.");
                return BadRequest(new ApiErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении поста.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse("Произошла ошибка при добавлении поста."));
            }
        }
    }
}