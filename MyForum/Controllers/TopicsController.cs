using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using MyForum.Services.TopicServices;

namespace MyForum.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ForumContext _context;
        private readonly ILogger<TopicsController> _logger;
        private readonly ITopicService _topicService;
        public TopicsController(ForumContext context, ILogger<TopicsController> logger,
            ITopicService topicService)
        {
            _context = context;
            _logger = logger;
            _topicService = topicService;
        }

        public async Task<IActionResult> Index(string categoryName, int topicId)
        {
            try
            {
                var topic = await _topicService.GetTopicByIdAsync(topicId);
                return View(topic);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении топика.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(int categoryId, string categoryName, string? title, string? content)
        {
            if (string.IsNullOrWhiteSpace(title))
                ModelState.AddModelError(nameof(title), "Введите название трэда.");
            else if (title.Length > 100)
                ModelState.AddModelError(nameof(title), "Длина не должна превышать больше 100 символов.");

            if (!ModelState.IsValid)
            {
                var category = await _context.Categories.Include(x => x.Topics).ThenInclude(x => x.User).FirstOrDefaultAsync(c => c.Id == categoryId);
                return View("~/Views/Categories/Index.cshtml", category);
            }

            try
            {
                await _topicService.CreateTopicAsync(categoryId, title, content, (int)User.GetUserId());
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) создал топик {title}.");
                int topicId = await _context.Topics.Where(t => t.Title == title).Select(t => t.Id).FirstOrDefaultAsync();
                var url = Url.Action("Index", "Topics", new { categoryName, topicId });
                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании топика.");
                return StatusCode(500, "Произошла ошибка при обработке запроса");
            }
        }

        [HttpPost]
        [Authorize(Policy = "OwnerOrAdmin")]
        public async Task<IActionResult> Delete(int topicId)
        {
            try
            {
                await _topicService.DeleteTopic(topicId);
                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) удалил топик {topicId}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении топика");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
