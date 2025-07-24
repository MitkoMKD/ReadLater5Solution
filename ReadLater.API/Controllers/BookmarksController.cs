using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReadLater.API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookmarksController : ControllerBase {
        private readonly IBookmarkService _bookmarkService;
        private readonly UserManager<IdentityUser> _userManager;
        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public BookmarksController(IBookmarkService bookmarkService, UserManager<IdentityUser> userManager) {
            _bookmarkService = bookmarkService;
            _userManager = userManager;
        }

        // GET: api/bookmarks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookmarkDto>>> GetBookmarks() {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Unauthorized();

            var bookmarks = await _bookmarkService.GetBookmarksForUser(CurrentUserId);
            return Ok(bookmarks.Select(ToDto));
        }

        // GET: api/bookmarks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookmarkDto>> GetBookmark(int id) {
            var bookmark = await _bookmarkService.GetByIdAsync(id);
            if (bookmark == null || bookmark.ApplicationUserId != CurrentUserId)
                return NotFound();

            return Ok(ToDto(bookmark));
        }

        // POST: api/bookmarks
        [HttpPost]
        public async Task<ActionResult<BookmarkDto>> PostBookmark([FromBody] BookmarkDto dto) {
            var bookmark = FromDto(dto, CurrentUserId);
            await _bookmarkService.CreateAsync(bookmark, CurrentUserId);
            return CreatedAtAction(nameof(GetBookmark), new { id = bookmark.ID }, ToDto(bookmark));
        }

        // PUT: api/bookmarks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookmark(int id, [FromBody] BookmarkDto dto) {
            if (id != dto.ID)
                return BadRequest();

            var bookmark = FromDto(dto, CurrentUserId);
            await _bookmarkService.UpdateAsync(bookmark);
            return NoContent();
        }

        // DELETE: api/bookmarks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookmark(int id) {
            await _bookmarkService.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }

        private static BookmarkDto ToDto(Bookmark b) => new() {
            ID = b.ID,
            Url = b.URL,
            ShortDescription = b.ShortDescription,
            CategoryId = (int)b.CategoryId
        };

        private static Bookmark FromDto(BookmarkDto dto, string userId) => new() {
            URL = dto.Url,
            ShortDescription = dto.ShortDescription,
            CategoryId = (int)dto.CategoryId,
            ApplicationUserId = userId
        };
    }
    public class BookmarkDto {
        public int ID { get; set; }
        public string Url { get; set; }
        public string ShortDescription { get; set; }
        public int CategoryId { get; set; }
    }
}
