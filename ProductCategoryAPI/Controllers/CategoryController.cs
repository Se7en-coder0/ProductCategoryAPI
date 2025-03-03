using Microsoft.AspNetCore.Mvc;
using ProductCategoryAPI.DTOs;
using ProductCategoryAPI.Services.Models;
using ProductCategoryAPI.Services.Repositories;

namespace ProductCategoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryRepository categoryRepository, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching categories - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            var categories = await _categoryRepository.GetAllCategoriesAsync(page, pageSize);

            if (!categories.Any()) return NoContent();

            var categoryDTOs = categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name
            });

            return Ok(categoryDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching category with ID {Id}", id);

            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            return Ok(new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            });
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CategoryDTO categoryDTO, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoryDTO == null) return BadRequest("Category data is required.");

            var category = new Category
            {
                Name = categoryDTO.Name
            };

            var newCategory = await _categoryRepository.AddCategoryAsync(category);

            _logger.LogInformation("Created new category with ID {Id}", newCategory.Id);

            var responseDTO = new CategoryDTO
            {
                Id = newCategory.Id,
                Name = newCategory.Name
            };

            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, responseDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] CategoryDTO categoryDTO, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoryDTO == null) return BadRequest("Category data is required.");
            if (id != categoryDTO.Id) return BadRequest("ID mismatch.");

            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(id);
            if (existingCategory == null) return NotFound();

            existingCategory.Name = categoryDTO.Name;
            await _categoryRepository.UpdateCategoryAsync(existingCategory);

            _logger.LogInformation("Updated category with ID {Id}", id);

            return Ok(new CategoryDTO
            {
                Id = existingCategory.Id,
                Name = existingCategory.Name
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting category with ID {Id}", id);

            var deleted = await _categoryRepository.DeleteCategoryAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
