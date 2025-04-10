using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Services;
using MyForum.Services.UserServices;

namespace MyForum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUserService _userService;
        public AdminController(ILogger<AdminController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult UserDetails()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserDetails(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError(nameof(username), "Введите никнейм пользователя.");
                return View();
            }

            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                ModelState.AddModelError(nameof(username), "Пользователь не найден.");
                return View();
            }

            return View(user);
        }
    }
}