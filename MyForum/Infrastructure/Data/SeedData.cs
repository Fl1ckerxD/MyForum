using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;

namespace MyForum.Infrastructure.Data
{
    public class SeedData
    {
        public static async Task SeedAsync(ForumDbContext context, ILogger logger, int retry = 0)
        {
            var retryForAvailability = retry;
            try
            {
                if (context.Database.IsNpgsql())
                    context.Database.Migrate();

                if (!await context.Boards.AnyAsync())
                {
                    context.Boards.AddRange(
                        new Board { ShortName = "b", Name = "Бред", Description = "Обсуждения всего на свете", Position = 1 },
                        new Board { ShortName = "vg", Name = "Видеоигры", Description = "Обсуждение игр", Position = 2 },
                        new Board { ShortName = "pr", Name = "Программирование", Description = "IT и программирование", Position = 3 }
                    );
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability >= 5) throw;

                retryForAvailability++;

                logger.LogError(ex.Message);
                await SeedAsync(context, logger, retryForAvailability);
                throw;
            }
        }
    }
}