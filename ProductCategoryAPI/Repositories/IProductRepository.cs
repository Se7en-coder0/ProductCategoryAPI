using ProductCategoryAPI.Models;

namespace ProductCategoryAPI.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(int page, int pageSize);
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product product, List<int> categoryIds);
        Task<Product> UpdateProductAsync(Product product, List<int> categoryIds);
        Task<bool> DeleteProductAsync(int id);
    }
}
