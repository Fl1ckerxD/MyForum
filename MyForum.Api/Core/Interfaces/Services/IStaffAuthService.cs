using MyForum.Api.Core.DTOs.Common;
using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IStaffAuthService
    {
        Task<AuthResult?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
        Task CreateAdminAsync(string username, string password, CancellationToken cancellationToken = default);
        Task CreateModeratorAsync(string username, string password, int boardId, ModeratorPermissions? permissions = null, CancellationToken cancellationToken = default);
    }
}