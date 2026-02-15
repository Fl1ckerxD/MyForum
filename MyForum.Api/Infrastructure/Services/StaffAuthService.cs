using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Enums;
using MyForum.Api.Core.Exceptions;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Infrastructure.Services
{
    public class StaffAuthService : IStaffAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<StaffAccount> _passwordHasher;
        private readonly ILogger<StaffAuthService> _logger;
        public StaffAuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher<StaffAccount> passwordHasher,
            ILogger<StaffAuthService> logger)
        {
            _uow = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Авторизация сотрудника системы
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns>Возвращает данные авторизованного сотрудника</returns>
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

        /// <summary>
        /// Создаёт новую учётную запись администратора
        /// </summary>
        public async Task CreateAdminAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            await CreateAsync(username, password, StaffRoles.Admin, null, null, cancellationToken);
        }

        /// <summary>
        /// Создаёт новую учётную запись модератора доски
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="boardId">Идентификатор доски</param>
        /// <param name="permissions">Права модератора (опционально, по умолчанию все права)</param>
        public async Task CreateModeratorAsync(string username, string password, int boardId, ModeratorPermissions? permissions = null, CancellationToken cancellationToken = default)
        {
            await CreateAsync(username, password, StaffRoles.Moderator, boardId, permissions, cancellationToken);
        }

        /// <summary>
        /// Внутренний метод создания учётной записи
        /// </summary>
        private async Task CreateAsync(
            string username,
            string password,
            StaffRoles role,
            int? boardId = null,
            ModeratorPermissions? permissions = null,
            CancellationToken cancellationToken = default)
        {
            // Проверка существования пользователя
            var existingAccount = await _uow.StaffAccounts.GetByUsernameAsync(username, cancellationToken);

            if (existingAccount != null)
            {
                _logger.LogWarning("Попытка создания дубликата учётной записи: {Username}", username);
                throw new UserAlreadyExistsException($"Пользователь с именем '{username}' уже существует");
            }

            // Валидация для модератора
            if (role == StaffRoles.Moderator)
            {
                if (!boardId.HasValue)
                    throw new ArgumentException("Для модератора необходимо указать BoardId", nameof(boardId));

                // Проверка существования доски
                var boardExists = await _uow.Boards.ExistsAsync(boardId.Value, cancellationToken);
                if (!boardExists)
                {
                    _logger.LogWarning("Попытка создания модератора для несуществующей доски: {BoardId}", boardId);
                    throw new BoardNotFoundException($"Доска с идентификатором {boardId} не найдена");
                }
            }

            // Создание новой учётной записи
            StaffAccount newAccount = role switch
            {
                StaffRoles.Admin => new Admin(),
                StaffRoles.Moderator => new BoardModerator
                {
                    BoardId = boardId.Value,
                    Permissions = permissions ?? new ModeratorPermissions()
                },
                _ => throw new ArgumentException($"Неизвестная роль: {role}", nameof(role))
            };

            newAccount.Username = username;
            newAccount.PasswordHash = _passwordHasher.HashPassword(newAccount, password);

            // Добавление и сохранение в БД
            try
            {
                await _uow.StaffAccounts.AddAsync(newAccount, cancellationToken);
                await _uow.SaveAsync(cancellationToken);

                _logger.LogInformation(
                    "Создана новая учётная запись: {Username}, Роль: {Role}, BoardId: {BoardId}",
                    username,
                    role,
                    boardId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении учётной записи: {Username}", username);
                throw new AccountCreationException("Ошибка при создании учётной записи", ex);
            }
        }
    }
}