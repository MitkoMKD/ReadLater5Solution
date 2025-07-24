using Data;
using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services {
    public class CategoryService : ICategoryService {
        private ReadLaterDataContext _readLaterDataContext;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ReadLaterDataContext readLaterDataContext, ILogger<CategoryService> logger) {
            _readLaterDataContext = readLaterDataContext;
            _logger = logger;
        }

        public async Task CreateCategoryAsync(Category category) {
            try {
                _readLaterDataContext.Add(category);
                await _readLaterDataContext.SaveChangesAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating category");
                throw; // Rethrow to allow the controller to show user-friendly error
            }
        }

        public async Task UpdateCategoryAsync(Category category) {
            try {
                _readLaterDataContext.Update(category);
                await _readLaterDataContext.SaveChangesAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error updating category with ID {Id}", category.ID);
                throw; // Rethrow to allow the controller to show user-friendly error
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync() {
            try {

                return await _readLaterDataContext.Categories
                        .AsNoTracking()
                        .ToListAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to fetch categories: {ErrorMessage}", ex.Message);
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<Category> GetCategoryAsync(int Id) {
            try {
                return await _readLaterDataContext.Categories.Where(c => c.ID == Id).FirstOrDefaultAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting category with ID {Id}", Id);
                return null;
            }
        }

        public async Task<Category> GetCategoryAsync(string Name) {
            try {
                return await _readLaterDataContext.Categories.Where(c => c.Name == Name).FirstOrDefaultAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting category with Name {Id}", Name);
                return null;
            }
        }

        public async Task DeleteCategoryAsync(int id, string userId) {
            try {
                var category = await _readLaterDataContext.Categories
                            .FirstOrDefaultAsync(c => c.ID == id && c.ApplicationUserId == userId);

                if (category == null)
                    throw new UnauthorizedAccessException("You don't own this category.");
                if (category != null) {
                    _readLaterDataContext.Categories.Remove(category);
                    await _readLaterDataContext.SaveChangesAsync();
                }
            } 
            catch (ObjectDisposedException ex) {
                _logger.LogError(ex, "DbContext has been disposed. Check for background thread usage.");
                throw;
            } 
            catch (Exception ex) {
                _logger.LogError(ex, "Error deleting category with ID {Id}", id);
                throw;
            }
        }
    }
}
