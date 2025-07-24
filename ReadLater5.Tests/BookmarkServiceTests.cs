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
    public class BookmarkServiceTests {
        private readonly Mock<ILogger<BookmarkService>> _mockLogger;
        private readonly DbContextOptions<ReadLaterDataContext> _options;

        public BookmarkServiceTests() {
            _mockLogger = new Mock<ILogger<BookmarkService>>();
            _options = new DbContextOptionsBuilder<ReadLaterDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreateAsync_ValidBookmark_SavesToDatabase() {
            // Arrange
            using var context = new ReadLaterDataContext(_options);
            var service = new BookmarkService(context, _mockLogger.Object);
            var bookmark = new Bookmark {
                URL = "https://example.com",
                ShortDescription = "Test",
                ApplicationUserId = "user1"
            };

            // Act
            await service.CreateAsync(bookmark, "user1");

            // Assert
            Assert.Single(context.Bookmarks);
        }

        [Fact]
        public async Task CreateAsync_InvalidCategoryId_ThrowsUnauthorized() {
            // Arrange
            using var context = new ReadLaterDataContext(_options);
            var service = new BookmarkService(context, _mockLogger.Object);
            var bookmark = new Bookmark {
                URL = "https://example.com",
                CategoryId = 99, // Invalid category
                ApplicationUserId = "user1"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.CreateAsync(bookmark, "user1")
            );
        }

        [Fact]
        public async Task GetBookmarksForUser_ReturnsOnlyOwnedBookmarks() {
            // Arrange
            using var context = new ReadLaterDataContext(_options);
            context.Bookmarks.AddRange(
                new Bookmark { ID = 1, ApplicationUserId = "user1" },
                new Bookmark { ID = 2, ApplicationUserId = "user2" }
            );
            await context.SaveChangesAsync();

            var service = new BookmarkService(context, _mockLogger.Object);

            // Act
            var result = await service.GetBookmarksForUser("user1");

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().ID);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_RemovesBookmark() {
            // Arrange
            using var context = new ReadLaterDataContext(_options);
            context.Bookmarks.Add(new Bookmark { ID = 1, ApplicationUserId = "user1" });
            await context.SaveChangesAsync();

            var service = new BookmarkService(context, _mockLogger.Object);

            // Act
            await service.DeleteAsync(1, "user1");

            // Assert
            Assert.Empty(context.Bookmarks);
        }

        [Fact]
        public async Task DeleteAsync_InvalidUser_ThrowsUnauthorized() {
            // Arrange
            using var context = new ReadLaterDataContext(_options);
            context.Bookmarks.Add(new Bookmark { ID = 1, ApplicationUserId = "user1" });
            await context.SaveChangesAsync();

            var service = new BookmarkService(context, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteAsync(1, "user2") // Wrong user
            );
        }
    }
}
