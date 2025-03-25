using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ForumContext _context;
        public AdminController(ForumContext context)
        {
            _context = context;
        }

        public IActionResult UserDetails()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserDetails(string? username)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError(nameof(username), "Введите никнейм пользователя.");
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Topics)
                .Include(u => u.Likes)
                .ThenInclude(u => u.Post)
                .ThenInclude(u => u.User)
                .Include(u => u.Posts)
                .ThenInclude(u => u.Likes)
                .FirstOrDefaultAsync(u => u.Username == username);

            if(user == null)
            {
                ModelState.AddModelError(nameof(username), "Пользователь не найден.");
                return View();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTopic(int topicId)
        {
            var topic = await _context.Topics.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic == null)
                return NotFound();

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserDetails", new { username = topic.User.Username });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.Include(t => t.User).FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserDetails", new { username = post.User.Username });
        }
    }
}
