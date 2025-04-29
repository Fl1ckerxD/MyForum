using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Services.PostServices;
using MyForum.Web.Extensions;
using MyForum.Web.Requests;

namespace MyForum.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PostsController> _logger;
        public PostsController(IPostService postService, ILogger<PostsController> logger, IUnitOfWork unitOfWork)
        {
            _postService = postService;
            _logger = logger;
            _uow = unitOfWork;
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
                var post = new Post
                {
                    TopicId = request.TopicId,
                    Content = request.Content,
                    UserId = User.GetUserId().Value
                };

                await _uow.Posts.AddAsync(post);
                await _uow.SaveAsync();

                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) добавил комментарий к топику {request.TopicId}.");

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
                await _postService.ToggleLikeAsync(postId, User.GetUserId().Value);
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) лайкнул пост {postId}.");

                return Json(new { likesCount = await _uow.Likes.GetLikesCountAsync(postId) });
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
                await _uow.Posts.DeleteAsync(postId);
                await _uow.SaveAsync();
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