using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyForum.Core.Entities;
using MyForum.Infrastructure.Data;
using MyForum.Services.CategoryServices;
using MyForum.Web.Controllers;

namespace MyForum.Tests.Controllers.Home
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsCategories()
        {
            // Arrange
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<HomeController>();
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
                HomeController controller = new(logger, new CategoryService(context));

                // Act
                ViewResult result = await controller.Index() as ViewResult;
                var categories = result.ViewData.Model as List<Category>;

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(categories);
                Assert.NotEmpty(result.Model as List<Category>);
            }
        }

        [Fact]
        public async Task Index_ReturnsCategoriesMoq()
        {
            // Arrange
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<HomeController>();
            var mockSet = new Mock<DbSet<Category>>();
            var mockContext = new Mock<ForumContext>();

            mockSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(new List<Category>
            {
                new Category { Name = "Technology" },
                new Category { Name = "Gaming" }
            }.AsQueryable().Provider);

            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(new List<Category>
            {
                new Category { Name = "Technology" },
                new Category { Name = "Gaming" }
            }.AsQueryable().Expression);

            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(typeof(Category));
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(new List<Category>
            {
                new Category { Name = "Technology" },
                new Category { Name = "Gaming" }
            }.GetEnumerator());

            mockContext.Setup(c => c.Categories).Returns(mockSet.Object);

            var controller = new HomeController(logger, new CategoryService(mockContext.Object));

            // Act
            var result = await controller.Index() as ViewResult;
            var categories = result.ViewData.Model as List<Category>;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(categories);
            Assert.Equal(2, categories.Count);
        }
    }
}
