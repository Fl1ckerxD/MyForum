using Microsoft.EntityFrameworkCore;
using MyForum.Api.Infrastructure.Data;

namespace MyForum.Tests
{
    public static class DbContext
    {
        public static DbContextOptions<ForumDbContext> GetOptions(string dbName = "TestDb")
        {
            return new DbContextOptionsBuilder<ForumDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        public static void Dispose(ForumDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
