using Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces {
    public interface IBookmarkService {
        Task<IEnumerable<Bookmark>> GetBookmarksForUser(string userId);
        Task<Bookmark> GetByIdAsync(int id);
        Task CreateAsync(Bookmark bookmark, string userId);
        Task UpdateAsync(Bookmark bookmark);
        Task DeleteAsync(int id, string userId);
    }
}
