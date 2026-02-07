using MyForum.Api.Core.DTOs.Common;

namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IStaffAuthService
    {
        Task<AuthResult?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
    }
}