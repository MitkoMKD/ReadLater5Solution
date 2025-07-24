using Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReadLater5.Controllers {
    public class BookmarksController : Controller {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBookmarkService _bookmarkService;
        private readonly ICategoryService _categoryService;
        private string _currentUserId => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        public BookmarksController(
            IBookmarkService bookmarkService,
            IHttpContextAccessor httpContextAccessor,
            ICategoryService categoryService) {
            _bookmarkService = bookmarkService;
            _httpContextAccessor = httpContextAccessor;
            _categoryService = categoryService;
        }

        // GET: Bookmarks
        public async Task<IActionResult> Index() {
            var bookmarks = await _bookmarkService.GetBookmarksForUser(_currentUserId);
            return View(bookmarks);
        }

        // GET: Bookmarks/Create
        public async Task<IActionResult> Create() {
            ViewBag.Categories = await _categoryService.GetCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bookmark bookmark) {
            if (ModelState.IsValid) {
                bookmark.ApplicationUserId = _currentUserId;
                await _bookmarkService.CreateAsync(bookmark,_currentUserId);
                return RedirectToAction(nameof(Index));
            }
            return View(bookmark);
        }

        // GET: Bookmarks/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) return NotFound();
            ViewBag.Categories = await _categoryService.GetCategoriesAsync();

            var bookmark = await _bookmarkService.GetByIdAsync(id.Value);
            if (bookmark == null || bookmark.ApplicationUserId != _currentUserId)
                return NotFound();

            return View(bookmark);
        }

        // POST: Bookmarks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bookmark bookmark) {
            if (id != bookmark.ID) return NotFound();

            if (ModelState.IsValid) {
                try {
                    bookmark.ApplicationUserId = _currentUserId;
                    await _bookmarkService.UpdateAsync(bookmark);
                } catch {
                    if (await _bookmarkService.GetByIdAsync(id) == null)
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bookmark);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) {
            try {
                await _bookmarkService.DeleteAsync(id, _currentUserId);
                return RedirectToAction("Index");
            } catch (UnauthorizedAccessException) {
                return Forbid(); // Shows 403 error
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> DetailsAsync(int? id) {
            if (id == null) {
                return new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }
            var category = await _bookmarkService.GetByIdAsync(id.Value);
            return category == null ? NotFound() : View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> DeleteAsync(int? id) {
            if (id == null) {
                return new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }
            Bookmark bookmark = await _bookmarkService.GetByIdAsync((int)id);
            if (bookmark == null) {
                return new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound);
            }
            return View(bookmark);
        }
    }
}
