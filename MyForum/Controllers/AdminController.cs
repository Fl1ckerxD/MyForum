using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Models;
using MyForum.Services;
using MyForum.Services.UserServices;

namespace MyForum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        public AdminController(ILogger<AdminController> logger, IUserService userService,
            IEntityService entityService)
        {
            _logger = logger;
            _userService = userService;
            _entityService = entityService;
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

        //[HttpPost]
        //public async Task<IActionResult> DeleteTopic(int topicId)
        //{
        //    try
        //    {
        //        await _entityService.DeleteEntityAsync(topicId, context => context.Topics);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при удалении топика.");
        //        return StatusCode(500, "Произошла ошибка при удалении топика.");
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> DeletePost(int postId)
        //{
        //    try
        //    {
        //        await _entityService.DeleteEntityAsync(postId, context => context.Posts);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при удалении топика.");
        //        return StatusCode(500, "Произошла ошибка при удалении топика.");
        //    }
        //}
    }
}