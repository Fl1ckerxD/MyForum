using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using MyForum.Services.PostServices;
using MyForum.Services.TopicServices;

namespace MyForum.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;
        private readonly ForumContext _context;
        public PostsController(IPostService postService, ILogger<PostsController> logger, ForumContext context)
        {
            _postService = postService;
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Comment([FromBody] CommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Комментарий не может быть пустым." });

            else if (request.Content.Length > 15000)
                return BadRequest(new { message = "Длина комментария не должна превышать 15000 символов." });

            try
            {
                await _postService.AddCommentAsync(request.TopicId, request.Content, (int)User.GetUserId());
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) добавил комментарий к топику {request.TopicId}.");

                var post = await _context.Posts.Where(p => p.UserId == (int)User.GetUserId()).OrderBy(p => p.Id).LastOrDefaultAsync();
                return Json(new
                {
                    id = post.Id,
                    username = User.Identity.Name,
                    createdAt = post.CreatedAt.ToString(),
                    content = post.Content
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении комментария.");
                return StatusCode(500, "Произошла ошибка при добавлении комментария.");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Like(int postId)
        {
            try
            {
                await _postService.ToggleLikeAsync(postId, (int)User.GetUserId());
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) лайкнул пост {postId}.");

                return Json(new { likesCount = _context.Likes.Where(l => l.PostId == postId).Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке лайка.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int postId)
        {
            try
            {
                await _postService.DeletePostAsync(postId);
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) удалил пост {postId}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении поста.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}

public record CommentRequest(int TopicId, string CategoryName, string Content);