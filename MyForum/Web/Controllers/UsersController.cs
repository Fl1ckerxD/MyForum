using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Services.UserServices;
using MyForum.Web.ViewModels;
using System.Security.Claims;

namespace MyForum.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly IUserRepository _userRepository;
        public UsersController(IUserService userService, IUserRepository userRepository,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
            _userRepository = userRepository;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!await _userRepository.IsUserNameUnique(model.Username))
                ModelState.AddModelError(nameof(model.Username), "Пользователь с таким именем уже существует.");

            if (!await _userRepository.IsEmailUnique(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Пользователь с тиким email уже существует.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _userService.CreateUserAsync(model);
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
