using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCategoryAPI.Services.Data;
using ProductCategoryAPI.Services.Models;

namespace ProductCategoryAPI.Services.Repositories
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
            _logger.LogInformation("Fetching categories - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

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
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Attempted to update non-existing category with ID {CategoryId}.", category.Id);
                return null;
            }

            if (existingCategory.Name == category.Name)
            {
                _logger.LogInformation("No changes detected for category ID {CategoryId}.", category.Id);
                return existingCategory;
            }

            existingCategory.Name = category.Name;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category '{CategoryName}' (ID: {CategoryId}) was updated.", category.Name, category.Id);
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
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
