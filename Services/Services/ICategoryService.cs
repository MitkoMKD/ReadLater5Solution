using Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services {
    public interface ICategoryService
    {
        Task CreateCategoryAsync(Category category);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<Category> GetCategoryAsync(int Id);
        Task<Category> GetCategoryAsync(string Name);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int category, string userId);
    }
}
