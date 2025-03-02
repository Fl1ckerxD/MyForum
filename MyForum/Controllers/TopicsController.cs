using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            var category = await _context.Categories.Include(x => x.Topics)
                .ThenInclude(x => x.User).ThenInclude(x => x.Posts).ThenInclude(x => x.Likes)
                .FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
                return NotFound();

            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == topicId);

            if (!category.Topics.Contains(topic))
                return NotFound();

            return View(category.Topics.FirstOrDefault(topic));
        }
    }
}
