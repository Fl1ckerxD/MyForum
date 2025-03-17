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
        public async Task<IActionResult> UserDetails(string username)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError(nameof(username), "Введите никнейм пользователя.");
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Topics)
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
    }
}
