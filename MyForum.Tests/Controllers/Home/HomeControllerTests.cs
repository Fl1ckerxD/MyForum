using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Core.Entities;
using MyForum.Infrastructure.Data;
using MyForum.Infrastructure.Repositories;
using MyForum.Web.Controllers;

namespace MyForum.Tests.Controllers.Home
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        
        public HomeControllerTests()
        {
            _mockLogger = new();
        }

        [Fact]
        public async Task Index_ReturnsCategories()
        {
            // Arrange
            var options = DbContext.GetOptions();

            using (var context = new ForumContext(options))
            {
                // Добавляем тестовые данные
                context.Categories.Add(new Category { Name = "Технологии", Description = "1" });
                context.Categories.Add(new Category { Name = "Игры", Description = "2" });
                await context.SaveChangesAsync();
            }

            using (var context = new ForumContext(options))
            {
                HomeController controller = new(_mockLogger.Object, new CategoryRepository(context));

                // Act
                ViewResult result = await controller.Index() as ViewResult;
                var categories = result.ViewData.Model;

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(categories);
            }
        }
    }
}
