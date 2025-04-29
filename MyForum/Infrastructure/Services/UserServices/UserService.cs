using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Web.ViewModels;

namespace MyForum.Infrastructure.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uof;
        private readonly IMemoryCache _cache;
        public UserService(IUnitOfWork uof, IMemoryCache cache)
        {
            _uof = uof;
            _cache = cache;
        }

        public async Task<User?> GetUserProfile(string username)
        {
            // пытаемся получить данные из кэша
            if (_cache.TryGetValue($"user_profile:{username}", out User cachedUser))
            {
                return cachedUser;
            }
            // обращаемся к базе данных
            var user = await _uof.Users.GetByUsernameOrEmailAsync(username);
            // если пользователь найден, то добавляем в кэш - время кэширования 5 минут
            if (user != null)
            {
                Console.WriteLine($"{user.Username} извлечен из базы данных");
                SaveUserProfileInCache(user, 5);
            }
            else
                Console.WriteLine($"{user.Username} извлечен из кэша");
            return user;
        }

        public void SaveUserProfileInCache(User user, int minutes)
        {
            _cache.Set($"user_profile:{user.Username}", user, TimeSpan.FromMinutes(minutes));
        }

        public async Task<User> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var hasher = new PasswordHasher<string>();
            var u = await _uof.Users.GetByUsernameOrEmailAsync(usernameOrEmail);
            if (u == null)
                return null;

            if (hasher.VerifyHashedPassword(null, u.Password, password) == PasswordVerificationResult.Success)
                return u;
            else
                return null;
        }

        public async Task CreateUserAsync(UserViewModel model)
        {
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = HashPassword(model.Password)
            };
            await _uof.Users.AddAsync(user);
            await _uof.SaveAsync();
        }

        private string HashPassword(string password)
        {
            var hasher = new PasswordHasher<string>();
            return hasher.HashPassword(null, password);
        }
    }
}
