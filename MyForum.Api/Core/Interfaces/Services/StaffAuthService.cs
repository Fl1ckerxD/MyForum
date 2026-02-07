using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;

namespace MyForum.Api.Core.Interfaces.Services
{
    public class StaffAuthService : IStaffAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<StaffAccount> _passwordHasher;
        public StaffAuthService(IUnitOfWork unitOfWork, IPasswordHasher<StaffAccount> passwordHasher)
        {
            _uow = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResult?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
        {
            var account = await _uow.StaffAccounts.GetByUsernameAsync(username, cancellationToken);

            if (account == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            var role = account switch
            {
                Admin => "Admin",
                BoardModerator => "Moderator",
                _ => "Unknown"
            };

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Name, account.Username),
                new(ClaimTypes.Role, role)
            };

            if (account is BoardModerator mod)
            {
                claims.Add(new Claim("BoardId", mod.BoardId.ToString()));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            return new AuthResult(
                principal,
                new AdminLoginResponse(
                    account.Id,
                    account.Username,
                    role
                )
            );
        }
    }
}