using Microsoft.EntityFrameworkCore;
using MyForum.Core.Entities;

namespace MyForum.Infrastructure.Data
{
    public class SeedData
    {
        public static async Task SeedAsync(ForumDbContext context, ILogger logger, int retry = 0)
        {
            const int maxRetryCount = 5;

            try
            {
                logger.LogInformation("Checking for pending migrations...");

                if (context.Database.IsNpgsql())
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                        await context.Database.MigrateAsync();
                    }
                    else
                    {
                        logger.LogInformation("Database is up to date, no migrations needed.");
                    }
                }

                logger.LogInformation("Checking for existing boards...");

                if (!await context.Boards.AnyAsync())
                {
                    logger.LogInformation("Seeding initial boards data...");

                    await context.Boards.AddRangeAsync(
                        new Board { ShortName = "b", Name = "Бред", Description = "Обсуждения всего на свете", Position = 1 },
                        new Board { ShortName = "vg", Name = "Видеоигры", Description = "Обсуждение игр", Position = 2 },
                        new Board { ShortName = "pr", Name = "Программирование", Description = "IT и программирование", Position = 3 }
                    );
                    await context.SaveChangesAsync();

                    logger.LogInformation("Successfully seeded {BoardCount} boards", 3);
                }
                else
                    logger.LogInformation("Boards already exist, skipping seeding");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database (Attempt {Retry}/{MaxRetry})", retry + 1, maxRetryCount);
                if (retry >= maxRetryCount - 1)
                {
                    logger.LogError("Max retry count reached, stopping seed attempts");
                    throw;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retry));
                logger.LogInformation("Waiting {Delay} before retry...", delay);
                await Task.Delay(delay);

                await SeedAsync(context, logger, retry + 1);
            }
        }
    }
}