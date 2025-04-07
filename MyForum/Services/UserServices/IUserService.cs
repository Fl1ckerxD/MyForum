using MyForum.Models;

namespace MyForum.Services.UserServices
{
    public interface IUserService
    {
        Task<User> GetUserByUsernameAsync(string username);
        bool IsUserNameUnique(string name);
        bool IsEmailUnique(string email);
        void SaveUserProfileInCache(User user, int minutes);
        Task<User?> GetUserProfile(string username);
    }
}
