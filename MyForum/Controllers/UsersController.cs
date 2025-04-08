using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyForum.Models;
using MyForum.Services.UserServices;
using System.Security.Claims;

namespace MyForum.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        public UsersController(ForumContext context, IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!await _userService.IsUserNameUnique(model.Username))
                ModelState.AddModelError(nameof(model.Username), "Пользователь с таким именем уже существует.");

            if (!await _userService.IsEmailUnique(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Пользователь с тиким email уже существует.");

            if (!ModelState.IsValid)
                return View(model); // Возвращаем форму с ошибками валидации
            try
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password
                };
                await _userService.CreateUserAsync(user);
                _logger.LogInformation($"Пользователь {model.Username} успешно зарегистрирован.");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании пользователя.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(username, password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
                    return View();
                }
                await SignInUserAsync(user);
                _userService.SaveUserProfileInCache(user, 5);
                _logger.LogInformation($"Пользователь {user.Username}({user.Id}) успешно вошел в систему.");
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                // Выполняем выход
                await HttpContext.SignOutAsync("Cookies");
                _logger.LogInformation("Пользователь успешно вышел из системы.");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выходе пользователя.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                string username = User.Identity.Name;

                var user = await _userService.GetUserProfile(username);

                if (user == null)
                    return NotFound();

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля пользователя.");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }

        private string HashPassword(string password)
        {
            var hasher = new PasswordHasher<string>();
            return hasher.HashPassword(null, password);
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
            });
        }
    }
}
