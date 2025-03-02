using Microsoft.AspNetCore.Mvc;
using ProductCategoryAPI.DTOs;
using ProductCategoryAPI.Models;
using ProductCategoryAPI.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCategoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync(page, pageSize);

            var categoryDTOs = categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return Ok(categoryDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            var categoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(categoryDTO);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Name = categoryDTO.Name
            };

            var newCategory = await _categoryRepository.AddCategoryAsync(category);

            var responseDTO = new CategoryDTO
            {
                Id = newCategory.Id,
                Name = newCategory.Name
            };

            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.Id }, responseDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != categoryDTO.Id) return BadRequest();

            var category = new Category
            {
                Id = categoryDTO.Id,
                Name = categoryDTO.Name
            };

            await _categoryRepository.UpdateCategoryAsync(category);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var deleted = await _categoryRepository.DeleteCategoryAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
