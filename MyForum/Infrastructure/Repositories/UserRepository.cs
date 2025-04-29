using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces;
using MyForum.Infrastructure.Data;

namespace MyForum.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ForumContext context) : base(context)
        {
        }

        public async Task<User> GetAllDetailsAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Topics)
                .Include(u => u.Likes).ThenInclude(u => u.Post).ThenInclude(u => u.User)
                .Include(u => u.Posts).ThenInclude(u => u.Likes)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                //.Include(u => u.Topics)
                //.Include(u => u.Likes).ThenInclude(u => u.Post).ThenInclude(u => u.User)
                //.Include(u => u.Posts).ThenInclude(u => u.Likes)
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        }

        public async Task<bool> IsEmailUnique(string email)
        {
            return !_context.Users.Any(u => u.Email == email);
        }

        public async Task<bool> IsUserNameUnique(string name)
        {
            return !_context.Users.Any(u => u.Username == name);
        }
    }
}
