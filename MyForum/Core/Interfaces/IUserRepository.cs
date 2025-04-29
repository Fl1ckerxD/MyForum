using MyForum.Core.Entities;

namespace MyForum.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> IsUserNameUnique(string name);
        Task<bool> IsEmailUnique(string email);
        Task<User> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<User> GetAllDetailsAsync(string username);
    }
}
