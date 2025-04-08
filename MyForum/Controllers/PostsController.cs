using Microsoft.AspNetCore.Authorization;
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
        private readonly ITopicService _topicService;
        private readonly ILogger<PostsController> _logger;
        public PostsController(IPostService postService, ITopicService topicService,
            ILogger<PostsController> logger)
        {
            _postService = postService;
            _topicService = topicService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Comment(string categoryName, int topicId, string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                ModelState.AddModelError(nameof(content), "Комментарий не должен быть пустым.");
            else if (content.Length > 15000)
                ModelState.AddModelError(nameof(content), "Длина не должна превышать больше 15000 символов.");

            if (!ModelState.IsValid)
            {
                var topic = await _topicService.GetTopicByIdAsync(topicId);
                return View("~/Views/Topics/Index.cshtml", topic);
            }

            try
            {
                await _postService.AddCommentAsync(topicId, content, (int)User.GetUserId());
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) добавил комментарий к топику {topicId}.");

                // Заменить на return Ok(); и на писать AJAX скрипт
                return Ok();
                //return RedirectToAction("Index", "Topics", new { categoryName, topicId });
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
                // Заменить на return Ok(); и на писать AJAX скрипт
                return Ok();
                //return RedirectToAction("Index", "Topics", new { categoryName = post.Topic.Category.Name, topicId = post.TopicId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке лайка.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        [HttpPost]
        [Authorize(Policy = "PostOwnerOrAdmin")]
        public async Task<IActionResult> Delete(int postId)
        {
            try
            {
                await _postService.DeletePostAsync(postId);
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) удалил пост {postId}.");

                // Заменить на return Ok(); и на писать AJAX скрипт
                return Ok();
                //return RedirectToAction("Index", "Topics", new { categoryName = post.Topic.Category.Name, topicId = post.TopicId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении поста.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
