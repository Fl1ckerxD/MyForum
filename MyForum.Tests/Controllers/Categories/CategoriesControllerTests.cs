using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Core.Entities;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Repositories;
using MyForum.Web.Controllers;

namespace MyForum.Tests.Controllers.Categories
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ILogger<CategoriesController>> _mockLogger;
        public CategoriesControllerTests()
        {
            _mockLogger = new();
        }

        [Fact]
        public async Task Index_ReturnsCategory()
        {
            // Arrange
            var options = DbContext.GetOptions();

            using (var context = new ForumContext(options))
            {
                // Добавление тестовых данных
                context.Categories.Add(new Category { Name = "Технологии", Description = "1" });

                context.Users.Add(new User { Username = "Tester", Email = "test@gmail.com", Password = "123", Role = "User" });

                context.Topics.Add(new Topic { Title = "1", CategoryId = 1, UserId = 1 });
                context.Topics.Add(new Topic { Title = "2", CategoryId = 1, UserId = 1 });

                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                CategoriesController controller = new(_mockLogger.Object, new CategoryRepository(context));

                // Act
                ViewResult result = await controller.Index("Технологии") as ViewResult;
                var category = result.Model as Category;

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(category);
            }
        }
    }
}