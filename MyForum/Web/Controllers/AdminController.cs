using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Core.Interfaces;

namespace MyForum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUnitOfWork _uow;
        public AdminController(ILogger<AdminController> logger, IUnitOfWork uow)
        {
            _logger = logger;
            _uow = uow;
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
            var user = await _uow.Users.GetAllDetailsAsync(username);

            if (user == null)
            {
                ModelState.AddModelError(nameof(username), "Пользователь не найден.");
                return View();
            }

            return View(user);
        }
    }
}