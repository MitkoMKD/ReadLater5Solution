using Data;
using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services {
    public class BookmarkService : IBookmarkService {

        private readonly ReadLaterDataContext _context;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(ReadLaterDataContext context, ILogger<BookmarkService> logger) {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(Bookmark bookmark, string userId) {
            try {
                if (bookmark.CategoryId != null) {
                    var categoryExists = await _context.Categories
                        .AnyAsync(c => c.ID == bookmark.CategoryId && c.ApplicationUserId == userId);
                    if (!categoryExists)
                        throw new UnauthorizedAccessException("Invalid CategoryId or access denied");
                }
                _context.Bookmarks.Add(bookmark);
                await _context.SaveChangesAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating bookmark");
                throw; // Rethrow to allow the controller to show user-friendly error
            }
        }

        public async Task DeleteAsync(int id, string userId) {
            try {
                var bookmark = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.ID == id && b.ApplicationUserId == userId);

                if (bookmark == null)
                    throw new UnauthorizedAccessException("You don't own this bookmark.");
                if (bookmark != null) {
                    _context.Bookmarks.Remove(bookmark);
                    await _context.SaveChangesAsync();
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting bookmark with ID {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Bookmark>> GetBookmarksForUser(string userId) {
            try {
                return await _context.Bookmarks
                    .AsNoTracking()
                    .Include(b => b.Category)
                    .Where(b => b.ApplicationUserId == userId)
                    .ToListAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting bookmarks for user {UserId}", userId);
                return Enumerable.Empty<Bookmark>();
            }
        }

        public async Task<Bookmark> GetByIdAsync(int id) {
            try {
                return await _context.Bookmarks
                    .Include(b => b.Category)
                    .FirstOrDefaultAsync(b => b.ID == id);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting bookmark with ID {Id}", id);
                return null;
            }
        }

        public async Task UpdateAsync(Bookmark bookmark) {
            try {
                _context.Bookmarks.Update(bookmark);
                await _context.SaveChangesAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error updating bookmark with ID {Id}", bookmark.ID);
                throw;
            }
        }
    }
}
