using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Web.Extensions;
using System.Net;

namespace MyForum.Web.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ILogger<TopicsController> _logger;
        private readonly IUnitOfWork _uow;
        public TopicsController(ILogger<TopicsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _uow = unitOfWork;
        }

        public async Task<IActionResult> Index(string categoryName, int topicId)
        {
            try
            {
                var topic = await _uow.Topics.GetByIdAsync(topicId);
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
                var url = Url.Action("Index", "Categories", new { categoryName });
                return Redirect(url);
            }

            try
            {
                var topic = new Topic
                {
                    Title = title,
                    Content = content,
                    CategoryId = categoryId,
                    UserId = (int)User.GetUserId()
                };
                await _uow.Topics.AddAsync(topic);
                await _uow.SaveAsync();

                _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) создал топик {title}.");

                return Redirect($"/{WebUtility.UrlEncode(categoryName)}/{topic.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании топика.");
                return StatusCode(500, "Произошла ошибка при обработке запроса");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int topicId)
        {
            try
            {
                await _uow.Topics.DeleteAsync(topicId);
                await _uow.SaveAsync();

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
