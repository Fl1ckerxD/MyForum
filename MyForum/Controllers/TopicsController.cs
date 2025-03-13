using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using System.Net;
using System.Security.Claims;

namespace MyForum.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ForumContext _context;
        public TopicsController(ForumContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string categoryName, int topicId)
        {
            var category = await _context.Categories.Include(x => x.Topics).FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
                return NotFound(); // Возвращаем 404, если категория не найдена

            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == topicId);

            if (!category.Topics.Contains(topic))
                return NotFound(); // Возвращаем 404, если тема не найдена

            topic = await _context.Topics.Include(x => x.User).Include(x => x.Posts).ThenInclude(x => x.Likes).Include(x => x.Posts).ThenInclude(x => x.User).FirstOrDefaultAsync(t => t.Id == topicId);
            return View(topic); // Передаем тему в представление
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

            var topic = new Topic
            {
                Title = title,
                Content = content,
                CategoryId = categoryId,
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            _context.Topics.Add(topic);
            _context.SaveChanges();

            return Redirect($"/{WebUtility.UrlEncode(categoryName)}/{topic.Id}");
        }
    }
}
