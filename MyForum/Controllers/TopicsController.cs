using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;

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

            topic = await _context.Topics.Include(x => x.User).Include(x => x.Posts).ThenInclude(x => x.User).ThenInclude(x => x.Likes).FirstOrDefaultAsync(t => t.Id == topicId);
            return View(topic); // Передаем тему в представление
        }
    }
}
