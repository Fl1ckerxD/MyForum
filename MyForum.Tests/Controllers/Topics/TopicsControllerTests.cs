using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Controllers;
using MyForum.Models;
using MyForum.Services;
using MyForum.Services.TopicServices;
using System.Security.Claims;

namespace MyForum.Tests.Controllers.Topics
{
    public class TopicsControllerTests
    {
        private readonly Mock<ForumContext> _mockContext;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<Logger<TopicsController>> _mockLogger;

        public TopicsControllerTests()
        {
            // Создаем ClaimsPrincipal для аутентифицированного пользователя
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            }, "TestAuth"));

            // Настройка HttpContext
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(h => h.User).Returns(principal);

            // Настройка ForumContext
            _mockContext = new Mock<ForumContext>(new DbContextOptions<ForumContext>());

            _mockLogger = new Mock<Logger<TopicsController>>();
        }

        [Fact]
        public async Task Index_ReturnsTopic()
        {
            // Arrange
            var options = DbContext.GetOptions();

            using (var context = new ForumContext(options))
            {
                // Добавление тестовых данных
                context.Categories.Add(new Category { Name = "Технологии", Description = "1" });

                context.Users.Add(new User { Username = "Tester", Email = "test@gmail.com", Password = "123", Role = "User" });

                context.Topics.Add(new Topic { Title = "1", CategoryId = 1, UserId = 1 });

                context.Posts.Add(new Post { TopicId = 1, UserId = 1, Content = "123", CreatedAt = DateTime.Now });
                context.Posts.Add(new Post { TopicId = 1, UserId = 1, Content = "123", CreatedAt = DateTime.Now });

                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                TopicsController controller = new TopicsController(context, _mockLogger.Object, new TopicService(context, new EntityService(context)));

                // Act
                ViewResult result = await controller.Index("Технологии", 1) as ViewResult;
                var topic = result.Model as Topic;

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(topic);
                Assert.NotEmpty(topic.Posts);
            }
        }

        [Fact]
        public async Task Create_WhenTitleIsEmpty_ReturnsVaildationError()
        {
            // Arrange
            var options = DbContext.GetOptions();

            using (var context = new ForumContext(options))
            {
                // Добавление тестовых данных
                context.Categories.Add(new Category { Name = "Технологии", Description = "1" });

                context.Users.Add(new User { Username = "Tester", Email = "test@gmail.com", Password = "123", Role = "User" });

                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                var controller = new TopicsController(context, _mockLogger.Object, new TopicService(context, new EntityService(context)));
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                };

                // Act
                var result = await controller.Create(
                    categoryId: 1,
                    categoryName: "tech",
                    title: "",
                    content: "Sample content");

                // Assert
                Assert.IsType<ViewResult>(result);
                var viewResult = result as ViewResult;
                Assert.Equal("~/Views/Categories/Index.cshtml", viewResult.ViewName);

                // Проверка наличия ошибки в ModelState
                Assert.Single(controller.ModelState.Values.SelectMany(v => v.Errors));
                Assert.Contains("Введите название трэда.", controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
        }

        [Fact]
        public async Task Create_WhenTitleTooLong_ReturnsValidationError()
        {
            // Arrange
            var options = DbContext.GetOptions();
            using (var context = new ForumContext(options))
            {
                // Добавление тестовых данных
                context.Categories.Add(new Category { Name = "Технологии", Description = "1" });

                context.Users.Add(new User { Username = "Tester", Email = "test@gmail.com", Password = "123", Role = "User" });

                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                var controller = new TopicsController(context, _mockLogger.Object, new TopicService(context, new EntityService(context)));
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                };

                // Act
                var result = await controller.Create(
                    categoryId: 1,
                    categoryName: "tech",
                    title: new string('a', 101),
                    content: "Sample content");

                // Assert
                Assert.IsType<ViewResult>(result);
                var viewResult = result as ViewResult;
                Assert.Equal("~/Views/Categories/Index.cshtml", viewResult.ViewName);

                // Проверка наличия ошибки в ModelState
                Assert.Single(controller.ModelState.Values.SelectMany(v => v.Errors));
                Assert.Contains("Длина не должна превышать больше 100 символов.", controller.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
        }

        [Fact]
        public async Task Delete_WhenTopicExistsAndUserIsOwner_ReturnsRedirect()
        {
            // Arrange
            var options = DbContext.GetOptions();

            using (var context = new ForumContext(options))
            {
                context.Categories.Add(new Category { Name = "Аниме", Description = "Sample" });
                context.Users.Add(new User { Username = "Tester", Email = "test@gmail.com", Password = "123", Role = "User" });
                context.Topics.Add(new Topic { CategoryId = 1, Title = "Sample title", Content = "", UserId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                var controller = new TopicsController(context, _mockLogger.Object, new TopicService(context, new EntityService(context)));
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                };

                // Act
                var result = await controller.Delete(3);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Categories", redirectResult.ControllerName);

                Assert.Null(await context.Topics.FirstOrDefaultAsync(t => t.Id == 1));
            }
        }

        [Fact]
        public async Task Delete_WhenUserNotOwner_ReturnNotFound()
        {
            // Arrange
            var options = DbContext.GetOptions();
            using (var context = new ForumContext(options))
            {
                context.Categories.Add(new Category { Name = "Аниме", Description = "Sample" });
                context.Users.Add(new User { Username = "Tester", Email = "test@gmail.com", Password = "123", Role = "User" });
                context.Users.Add(new User { Username = "Tester2", Email = "test2@gmail.com", Password = "123", Role = "User" });
                context.Topics.Add(new Topic { CategoryId = 1, Title = "Sample title", Content = "", UserId = 2 });
                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                var controller = new TopicsController(context, _mockLogger.Object, new TopicService(context, new EntityService(context)));
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                };

                // Act
                var result = await controller.Delete(4);

                // Assert
                Assert.IsType<ForbidResult>(result);
            }
        }
    }
}
