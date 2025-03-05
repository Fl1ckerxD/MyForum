using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyForum.Models;
using MyForum.Services;
using System.Security.Claims;

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

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => (u.Username == username || u.Email == username) && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");

            // Создаем ClaimsPrincipal и выполняем вход
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true, // Сохраняем вход после закрытия браузера
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1) // Время жизни сессии
            });

            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> Logout()
        {
            // Выполняем выход
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем ID пользователя 

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            return View(user);
        }
    }
}
