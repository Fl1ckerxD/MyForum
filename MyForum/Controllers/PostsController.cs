using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using System.Security.Claims;

namespace MyForum.Controllers
{
    public class PostsController : Controller
    {
        private readonly ForumContext _context;
        public PostsController(ForumContext context)
        {
            _context = context;
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
                var topic = await _context.Topics.Include(x => x.User).Include(x => x.Category).Include(x => x.Posts).ThenInclude(x => x.User).ThenInclude(x => x.Likes).FirstOrDefaultAsync(t => t.Id == topicId);
                return View("~/Views/Topics/Index.cshtml", topic);
            }

            var post = new Post
            {
                Content = content,
                TopicId = topicId,
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            _context.Posts.Add(post);
            _context.SaveChanges();

            return RedirectToAction("Index", "Topics", new { categoryName, topicId });
        }

        [HttpPost]
        public async Task<IActionResult> Like(int postId)
        {
            var post = await _context.Posts.Include(p => p.Topic).ThenInclude(p => p.Category).Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                // Если пользователь уже лайкнул, убрать лайк
                _context.Likes.Remove(existingLike);
            }

            else
            {
                var newLike = new Like
                {
                    PostId = postId,
                    UserId = userId
                };
                _context.Likes.Add(newLike);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Topics", new { categoryName = post.Topic.Category.Name, topicId = post.TopicId });
        }
    }
}
