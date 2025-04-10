using MyForum.Models;

namespace MyForum.Services.UserServices
{
    public interface IUserService
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> IsUserNameUnique(string name);
        Task<bool> IsEmailUnique(string email);
        Task<User?> GetUserProfile(string username);
        Task<User> AuthenticateAsync(string usernameOrEmail, string password);
        Task CreateUserAsync(User user);
        Task CreateUserAsync(UserViewModel user);
        void SaveUserProfileInCache(User user, int minutes);
    }
}
