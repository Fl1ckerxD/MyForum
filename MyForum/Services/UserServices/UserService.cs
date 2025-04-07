using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyForum.Models;

namespace MyForum.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly ForumContext _context;
        private readonly IMemoryCache _cache;
        public UserService(ForumContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (_cache.TryGetValue($"user:{username}", out User cachedUser))
            {
                return cachedUser;
            }

            var user = await _context.Users
                .Include(u => u.Topics)
                .Include(u => u.Likes)
                .ThenInclude(u => u.Post)
                .ThenInclude(u => u.User)
                .Include(u => u.Posts)
                .ThenInclude(u => u.Likes)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                _cache.Set($"user:{username}", user, TimeSpan.FromMinutes(10));
            }

            return user;
        }

        public bool IsUserNameUnique(string name)
        {
            return !_context.Users.Any(u => u.Username == name);
        }

        public bool IsEmailUnique(string email)
        {
            return !_context.Users.Any(u => u.Email == email);
        }

        public async Task<User?> GetUserProfile(string username)
        {
            // пытаемся получить данные из кэша
            if (_cache.TryGetValue($"user_profile:{username}", out User cachedUser))
            {
                return cachedUser;
            }
            // обращаемся к базе данных
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
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
    }
}
