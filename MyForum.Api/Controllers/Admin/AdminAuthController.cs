using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IStaffAuthService _authService;
        private readonly IValidator<AdminLoginRequest> _loginRequestValidator;

        public AdminAuthController(IStaffAuthService authService, IValidator<AdminLoginRequest> loginRequestValidator)
        {
            _authService = authService;
            _loginRequestValidator = loginRequestValidator;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AdminLoginResponse>> Login(
            [FromBody] AdminLoginRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = _loginRequestValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _authService.AuthenticateAsync(
                request.Username,
                request.Password,
                cancellationToken);

            if (result is null)
                return Unauthorized("Неверное имя пользователя или пароль.");

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                result.Principal);

            return Ok(result.Response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = User.Identity?.Name,
                Role = User.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}