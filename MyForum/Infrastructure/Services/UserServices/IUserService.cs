using MyForum.Core.Entities;
using MyForum.Web.ViewModels;

namespace MyForum.Infrastructure.Services.UserServices
{
    public interface IUserService
    {
        Task<User?> GetUserProfile(string username);
        Task<User> AuthenticateAsync(string usernameOrEmail, string password);
        Task CreateUserAsync(UserViewModel user);
        void SaveUserProfileInCache(User user, int minutes);
    }
}
