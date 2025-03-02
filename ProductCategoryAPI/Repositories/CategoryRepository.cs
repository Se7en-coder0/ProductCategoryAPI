using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCategoryAPI.Data;
using ProductCategoryAPI.Models;

namespace ProductCategoryAPI.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(int page, int pageSize)
        {
            return await _context.Categories
                .OrderBy(c => c.Id) // Ensures consistent paging order
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} was not found.", id);
            }
            return category;
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) was created.", category.Name, category.Id);
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Attempted to update non-existing category with ID {CategoryId}.", category.Id);
                return null;
            }

            existingCategory.Name = category.Name;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) was updated.", category.Name, category.Id);
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Attempted to delete non-existing category with ID {CategoryId}.", id);
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Category with ID {CategoryId} was deleted.", id);
            return true;
        }
    }
}
