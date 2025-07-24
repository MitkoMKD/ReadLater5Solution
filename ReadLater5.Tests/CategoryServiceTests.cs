using Data;
using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ReadLater5.Tests {
    public class CategoryServiceTests : IDisposable {
        private readonly Mock<ILogger<CategoryService>> _mockLogger;
        private readonly ReadLaterDataContext _context;

        public CategoryServiceTests() {
            _mockLogger = new Mock<ILogger<CategoryService>>();
            var options = new DbContextOptionsBuilder<ReadLaterDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ReadLaterDataContext(options);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task CreateCategoryAsync_ValidCategory_SavesToDatabase() {
            // Arrange
            var service = new CategoryService(_context, _mockLogger.Object);
            var category = new Category { Name = "Test", ApplicationUserId = "user1" };

            // Act
            await service.CreateCategoryAsync(category);

            // Assert
            Assert.Single(_context.Categories);
            Assert.Equal("Test", _context.Categories.First().Name);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsAllCategories() {
            // Arrange
            _context.Categories.AddRange(
                new Category { ID = 1, Name = "Tech" },
                new Category { ID = 2, Name = "News" }
            );
            await _context.SaveChangesAsync();

            var service = new CategoryService(_context, _mockLogger.Object);

            // Act
            var result = await service.GetCategoriesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCategoryAsync_ById_ReturnsCorrectCategory() {
            // Arrange
            _context.Categories.Add(new Category { ID = 1, Name = "Tech" });
            await _context.SaveChangesAsync();

            var service = new CategoryService(_context, _mockLogger.Object);

            // Act
            var result = await service.GetCategoryAsync(1);

            // Assert
            Assert.Equal("Tech", result.Name);
        }

        [Fact]
        public async Task GetCategoryAsync_ByName_ReturnsCorrectCategory() {
            // Arrange
            _context.Categories.Add(new Category { ID = 1, Name = "Tech" });
            await _context.SaveChangesAsync();

            var service = new CategoryService(_context, _mockLogger.Object);

            // Act
            var result = await service.GetCategoryAsync("Tech");

            // Assert
            Assert.Equal(1, result.ID);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ValidUser_DeletesCategory() {
            // Arrange
            _context.Categories.Add(new Category { ID = 1, ApplicationUserId = "user1" });
            await _context.SaveChangesAsync();

            var service = new CategoryService(_context, _mockLogger.Object);

            // Act
            await service.DeleteCategoryAsync(1, "user1");

            // Assert
            Assert.Empty(_context.Categories);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WrongUser_ThrowsUnauthorized() {
            // Arrange
            _context.Categories.Add(new Category { ID = 1, ApplicationUserId = "user1" });
            await _context.SaveChangesAsync();

            var service = new CategoryService(_context, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteCategoryAsync(1, "user2")
            );
        }

        [Fact]
        public async Task DeleteCategoryAsync_DisposedContext_ThrowsAndLogs() {
            // Arrange
            var disposedContext = new ReadLaterDataContext(new DbContextOptions<ReadLaterDataContext>());
            disposedContext.Dispose();

            var service = new CategoryService(disposedContext, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(
                () => service.DeleteCategoryAsync(1, "user1")
            );
            _mockLogger.VerifyLog(LogLevel.Error, "DbContext has been disposed");
        }
    }

    // Logger verification helper
    public static class MockLoggerExtensions {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, string message) {
            logger.Verify(x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ));
        }
    }
}