using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCategoryAPI.Data;
using ProductCategoryAPI.Models;

namespace ProductCategoryAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(int page, int pageSize)
        {
            return await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .OrderBy(p => p.Id) // Ensures consistent paging order
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found.", id);
            }
            return product;
        }

        public async Task<Product> AddProductAsync(Product product, List<int> categoryIds)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            foreach (var categoryId in categoryIds)
            {
                _context.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Product '{ProductName}' (ID: {ProductId}) was created with categories: {CategoryIds}",
                product.Name, product.Id, string.Join(", ", categoryIds));

            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product, List<int> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count == 0)
            {
                _logger.LogWarning("Attempted to update product '{ProductName}' (ID: {ProductId}) with an empty category list.",
                    product.Name, product.Id);
                return null;
            }

            var existingProduct = await _context.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
            {
                _logger.LogWarning("Attempted to update non-existing product with ID {ProductId}.", product.Id);
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;

            // Update categories without clearing existing data unnecessarily
            existingProduct.ProductCategories = existingProduct.ProductCategories
                .Where(pc => categoryIds.Contains(pc.CategoryId))
                .ToList();

            foreach (var categoryId in categoryIds.Except(existingProduct.ProductCategories.Select(pc => pc.CategoryId)))
            {
                existingProduct.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Product '{ProductName}' (ID: {ProductId}) was updated. New categories: {CategoryIds}",
                existingProduct.Name, existingProduct.Id, string.Join(", ", categoryIds));

            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product with ID {Id} was deleted.", id);
            return true;
        }
    }
}
