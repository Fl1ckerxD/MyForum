using MyForum.Api.Infrastructure.Data;
using MyForum.Api.Infrastructure.Repositories;

namespace MyForum.Api.Tests.Repositories
{
    public class PostRepositoryTests
    {
        private readonly PostRepository _postRepository;
        private readonly ForumDbContext _context;

        public PostRepositoryTests()
        {
            var options = DbContext.GetOptions(nameof(ThreadRepositoryTests));
            _context = new ForumDbContext(options);
            _postRepository = new PostRepository(_context);
        }
    }
}