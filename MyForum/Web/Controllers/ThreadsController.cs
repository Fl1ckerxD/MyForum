using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Web.Controllers
{
    public class ThreadsController : Controller
    {
        private readonly ILogger<ThreadsController> _logger;
        private readonly IThreadService _threadService;

        public ThreadsController(ILogger<ThreadsController> logger, IThreadService threadService)
        {
            _logger = logger;
            _threadService = threadService;
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
                // var topic = new Topic
                // {
                //     Title = title,
                //     Content = content,
                //     CategoryId = categoryId,
                //     UserId = (int)User.GetUserId()
                // };
                // await _uow.Topics.AddAsync(topic);
                // await _uow.SaveAsync();

                // _logger.LogInformation($"Пользователь {User.Identity.Name}({User.GetUserId()}) создал топик {title}.");

                //return Redirect($"/{WebUtility.UrlEncode(categoryName)}/{topic.Id}");
                return RedirectToAction("Index", "Categories", new { categoryName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании топика.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
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
    }
}
