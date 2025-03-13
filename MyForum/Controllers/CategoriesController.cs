using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using System.Net;
using System.Security.Claims;

namespace MyForum.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ForumContext _context;
        public CategoriesController(ForumContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string categoryName)
        {
            var category = await _context.Categories.Include(x => x.Topics).ThenInclude(x => x.User).FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
            {
                return NotFound(); // Возвращаем 404, если категория не найдена
            }

            return View(category); // Передаем категорию в представление
        }
    }
}
