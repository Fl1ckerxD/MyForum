using Microsoft.EntityFrameworkCore;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Services;
using MyForum.Api.Infrastructure.Services;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Infrastructure.Data
{
    public class SeedData
    {
        public static async Task SeedAsync(ForumDbContext context, ILogger logger, IIPHasher ipHasher, int retry = 0)
        {
            const int maxRetryCount = 5;

            try
            {
                #region Migrations
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
                #endregion

                #region Seed Boards
                logger.LogInformation("Checking for existing boards...");

                if (!await context.Boards.AnyAsync())
                {
                    logger.LogInformation("Seeding initial boards data...");

                    var Boards = new List<Board>
                    {
                        new Board { ShortName = "b", Name = "Бред", Description = "Обсуждения всего на свете", Position = 1 },
                        new Board { ShortName = "vg", Name = "Видеоигры", Description = "Обсуждение игр", Position = 2 },
                        new Board { ShortName = "pr", Name = "Программирование", Description = "IT и программирование", Position = 3 }
                    };

                    await context.Boards.AddRangeAsync(Boards);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Successfully seeded {BoardCount} boards", Boards.Count);
                }
                else
                    logger.LogInformation("Boards already exist, skipping seeding");
                #endregion

                #region Seed Threads
                logger.LogInformation("Checking for existing threads...");

                if (!await context.Threads.AnyAsync())
                {
                    logger.LogInformation("Seeding initial threads data...");

                    var boardB = await context.Boards.FirstAsync(b => b.ShortName == "b");
                    var boardVg = await context.Boards.FirstAsync(b => b.ShortName == "vg");
                    var boardPr = await context.Boards.FirstAsync(b => b.ShortName == "pr");

                    var Threads = new List<Thread>
                    {
                        new Thread
                        {
                            Subject = "Почему кошки так странно себя ведут?",
                            IsPinned = false,
                            IsLocked = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-5),
                            LastBumpAt = DateTime.UtcNow.AddHours(-1),
                            BoardId = boardB.Id
                        },
                        new Thread
                        {
                            Subject = "[ПРАВИЛА] Правила раздела и рекомендации",
                            IsPinned = true,
                            IsLocked = false,
                            CreatedAt = DateTime.UtcNow.AddDays(-2),
                            LastBumpAt = DateTime.UtcNow.AddHours(-3),
                            BoardId = boardVg.Id
                        },
                        new Thread
                        {
                            Subject = "Обсуждение лучших практик REST API",
                            IsPinned = false,
                            IsLocked = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-1),
                            LastBumpAt = DateTime.UtcNow.AddHours(-6),
                            BoardId = boardPr.Id
                        },
                        new Thread
                        {
                            Subject = "Что думаете о новой игре от CDPR?",
                            IsPinned = false,
                            IsLocked = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-10),
                            LastBumpAt = DateTime.UtcNow.AddMinutes(-30),
                            BoardId = boardVg.Id
                        },
                        new Thread
                        {
                            Subject = "Помогите с выбором фреймворка для фронтенда",
                            IsPinned = false,
                            IsLocked = false,
                            CreatedAt = DateTime.UtcNow.AddHours(-3),
                            LastBumpAt = DateTime.UtcNow.AddMinutes(-15),
                            BoardId = boardPr.Id
                        }
                    };

                    await context.Threads.AddRangeAsync(Threads);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Successfully seeded {ThreadCount} threads", Threads.Count);
                }
                else
                    logger.LogInformation("Threads already exist, skipping seeding");
                #endregion

                #region Seed Posts
                logger.LogInformation("Checking for existing posts...");

                if (!await context.Posts.AnyAsync())
                {
                    logger.LogInformation("Seeding initial posts data...");

                    var threads = await context.Threads.ToListAsync();

                    var Posts = new List<Post>();

                    // Для каждого треда создаем оригинальный пост и несколько ответов
                    foreach (var thread in threads)
                    {
                        // Оригинальный пост (ОП)
                        var originalPost = new Post
                        {
                            AuthorName = "Аноним",
                            AuthorTripcode = null,
                            Content = GetOriginalPostContent(thread.Subject),
                            CreatedAt = thread.CreatedAt,
                            ThreadId = thread.Id,
                            IpAddress = ipHasher.HashIP("127.0.0.1")
                        };
                        Posts.Add(originalPost);

                        // Сохраняем оригинальный пост в треде
                        thread.OriginalPost = originalPost;

                        for (int i = 1; i <= 3; i++)
                        {
                            Posts.Add(new Post
                            {
                                AuthorName = GetRandomAuthorName(),
                                AuthorTripcode = i == 1 ? "!tripcode" : null,
                                AuthorId = i == 2 ? "ID:AbCd12" : null,
                                Content = GetReplyContent(thread.Subject, i),
                                CreatedAt = thread.CreatedAt.AddMinutes(i * 10),
                                ThreadId = thread.Id,
                                ReplyToPost = originalPost,
                                IpAddress = ipHasher.HashIP("192.168.1." + i)
                            });
                        }

                        // Ответ на ответ (вложенная дискуссия)
                        if (thread.Id % 3 != 0)
                        {
                            Posts.Add(new Post
                            {
                                AuthorName = "Скептик",
                                Content = "А есть доказательства этому утверждению?",
                                CreatedAt = thread.CreatedAt.AddMinutes(45),
                                ThreadId = thread.Id,
                                ReplyToPost = Posts.Last(p => p.ThreadId == thread.Id && p != originalPost),
                                IpAddress = ipHasher.HashIP("10.0.0.1")
                            });
                        }
                    }

                    // Обновляем счетчики в тредах
                    foreach (var thread in threads)
                    {
                        var postCount = Posts.Count(p => p.ThreadId == thread.Id);
                        thread.PostCount = postCount;
                        thread.ReplyCount = postCount - 1; // минус ОП
                        thread.FileCount = 0;
                    }

                    await context.Posts.AddRangeAsync(Posts);
                    await context.SaveChangesAsync();

                    logger.LogInformation("Successfully seeded {PostCount} posts", Posts.Count);
                }
                else
                    logger.LogInformation("Posts already exist, skipping seeding");
                #endregion
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

                await SeedAsync(context, logger, ipHasher, retry + 1);
            }
        }

        private static string GetOriginalPostContent(string subject)
        {
            var contents = new Dictionary<string, string>
                {
                    { "кошки", "Ребят, у меня кошка последнюю неделю постоянно бегает по квартире в 3 часа ночи и скидывает все со столов. Это нормально? Может, ей чего-то не хватает? Даю и корм, и воду, в туалет ходит нормально." },
                    { "ПРАВИЛА", "Добро пожаловать в раздел видеоигр!\n\nОсновные правила:\n1. Без флейма и оскорблений\n2. Новости публикуем с источником\n3. Спойлеры в тегах [спойлер]\n4. Обсуждение пиратства запрещено\n\nРекомендуемые ресурсы для новостей: StopGame, DTF, Kanobu." },
                    { "REST API", "Собираем здесь лучшие практики по проектированию RESTful API. Начну с основных:\n- Используйте правильные HTTP-методы\n- Версионирование через заголовки\n- Пагинация для больших коллекций\n- Статус-коды должны быть осмысленными\n\nКакие еще практики считаете обязательными?" },
                    { "CDPR", "Только посмотрел трейлер новой Cyberpunk Orion. Выглядит круто, но после релиза 2077 немного скептически отношусь. Что думаете? Стоит ждать или уже не доверяете студии?" },
                    { "фреймворка", "Выбираю между React, Vue и Svelte для нового проекта. Требования: быстрая разработка, большое комьюнити, готовые UI-киты. React знаком, но хочется попробовать что-то новое. Посоветуйте!" }
                };

            foreach (var key in contents.Keys)
            {
                if (subject.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return contents[key];
            }

            return "Оригинальное сообщение треда. Обсуждаем тему: " + subject;
        }

        private static string GetReplyContent(string subject, int replyNumber)
        {
            var replies = new List<string>
                {
                    "Полностью согласен с автором, сам сталкивался с подобной проблемой.",
                    "Мне кажется, вы упускаете важный момент в своих рассуждениях.",
                    "А можно поподробнее? Не совсем понял вашу точку зрения.",
                    "Источник? Без доказательств это просто ничем не подкрепленное утверждение.",
                    "Спасибо за информацию, очень полезно! Возьму на заметку.",
                    "На самом деле все не так однозначно, как кажется на первый взгляд.",
                    "Проверял на личном опыте - работает именно так, как описано."
                };

            return replies[replyNumber % replies.Count] + " (ответ #" + replyNumber + ")";
        }

        private static string GetRandomAuthorName()
        {
            var names = new[] { "Аноним", "Гость", "Специалист", "Новичок", "Опытный", "Читатель", "Сомневающийся" };
            return names[new Random().Next(names.Length)];
        }
    }
}