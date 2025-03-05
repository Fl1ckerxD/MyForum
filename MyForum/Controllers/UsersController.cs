using Microsoft.AspNetCore.Mvc;
using MyForum.Models;
using MyForum.Services;

namespace MyForum.Controllers
{
    public class UsersController : Controller
    {
        private readonly ForumContext _context;
        private readonly UserService _userService;
        public UsersController(ForumContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!_userService.IsUserNameUnique(model.Username))
                ModelState.AddModelError(nameof(model.Username), "Пользователь с таким именем уже существует.");

            if (!_userService.IsEmailUnique(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Пользователь с тиким email уже существует.");

            if (!ModelState.IsValid)
                return View(model); // Возвращаем форму с ошибками валидации

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
