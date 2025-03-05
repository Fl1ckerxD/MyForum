using MyForum.Models;

namespace MyForum.Services
{
    public class UserService
    {
        private readonly ForumContext _context;
        public UserService(ForumContext context)
        {
            _context = context;
        }

        public bool IsUserNameUnique(string name)
        {
            return !_context.Users.Any(u => u.Username == name);
        }

        public bool IsEmailUnique(string email)
        {
            return !_context.Users.Any(u => u.Email == email);
        }
    }
}
