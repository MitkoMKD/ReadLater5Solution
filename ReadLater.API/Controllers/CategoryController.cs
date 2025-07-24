using Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using ReadLater5.Controllers;

namespace ReadLater.API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        private string _currentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public CategoryController(
            ICategoryService categoryService,
            ILogger<CategoryController> logger) {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetCategories() {
            try {
                var categories = await _categoryService.GetCategoriesAsync();
                return Ok(categories);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error fetching categories");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id) {
            try {
                var category = await _categoryService.GetCategoryAsync(id);

                if (category == null)
                    return NotFound();

                if (category.ApplicationUserId != _currentUserId)
                    return Forbid();

                return Ok(category);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error fetching category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category category) {
            try {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                category.ApplicationUserId = _currentUserId;
                await _categoryService.CreateCategoryAsync(category);

                return CreatedAtAction(nameof(GetCategory), new { id = category.ID }, category);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category) {
            try {
                if (id != category.ID)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCategory = await _categoryService.GetCategoryAsync(id);
                if (existingCategory == null)
                    return NotFound();

                if (existingCategory.ApplicationUserId != _currentUserId)
                    return Forbid();

                await _categoryService.UpdateCategoryAsync(category);
                return NoContent();
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error updating category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id) {
            try {
                var category = await _categoryService.GetCategoryAsync(id);
                if (category == null)
                    return NotFound();

                if (category.ApplicationUserId != _currentUserId)
                    return Forbid();

                await _categoryService.DeleteCategoryAsync(id, _currentUserId);
                return NoContent();
            } catch (UnauthorizedAccessException) {
                return Forbid();
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error deleting category with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
