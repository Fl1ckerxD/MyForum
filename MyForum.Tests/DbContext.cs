using Microsoft.EntityFrameworkCore;
using MyForum.Models;

namespace MyForum.Tests
{
    public static class DbContext
    {
        public static DbContextOptions<ForumContext> GetOptions()
        {
            return new DbContextOptionsBuilder<ForumContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }
    }
}
