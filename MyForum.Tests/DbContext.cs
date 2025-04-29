using Microsoft.EntityFrameworkCore;
using MyForum.Infrastructure.Data;

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

        public static void Dispose(ForumContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
