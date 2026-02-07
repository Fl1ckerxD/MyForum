using MyForum.Api.Core.Entities;

namespace MyForum.Api.Core.Interfaces.Repositories
{
    public interface IStaffAccountRepository
    {
        Task<StaffAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}