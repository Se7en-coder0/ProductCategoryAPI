using Microsoft.AspNetCore.Mvc;
using ProductCategoryAPI.DTOs;
using ProductCategoryAPI.Services.Models;
using ProductCategoryAPI.Services.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ProductCategoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching products - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            var products = await _productRepository.GetAllProductsAsync(page, pageSize);
            if (!products.Any()) return NoContent();

            var productDTOs = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Categories = p.ProductCategories.Select(pc => new CategoryDTO
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name
                }).ToList()
            }).ToList();

            return Ok(productDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching product with ID {Id}", id);

            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return Ok(new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Categories = product.ProductCategories.Select(pc => new CategoryDTO
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (request.CategoryIds == null || request.CategoryIds.Count == 0)
                return BadRequest("A product must have at least one category.");

            var product = new Product
            {
                Name = request.ProductDTO.Name,
                Description = request.ProductDTO.Description,
                Price = request.ProductDTO.Price
            };

            var newProduct = await _productRepository.AddProductAsync(product, request.CategoryIds);
            _logger.LogInformation("Created new product with ID {Id}", newProduct.Id);

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, new ProductDTO
            {
                Id = newProduct.Id,
                Name = newProduct.Name,
                Description = newProduct.Description,
                Price = newProduct.Price,
                Categories = request.CategoryIds.Select(id => new CategoryDTO
                {
                    Id = id,
                    Name = "Category Placeholder"
                }).ToList()
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != request.ProductDTO.Id) return BadRequest("Product ID in URL and request body must match.");
            if (request.CategoryIds == null || request.CategoryIds.Count == 0)
                return BadRequest("A product must have at least one category.");

            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = request.ProductDTO.Name;
            existingProduct.Description = request.ProductDTO.Description;
            existingProduct.Price = request.ProductDTO.Price;

            var updatedProduct = await _productRepository.UpdateProductAsync(existingProduct, request.CategoryIds);
            if (updatedProduct == null) return NotFound();

            _logger.LogInformation("Updated product with ID {Id}", id);
            return Ok(new ProductDTO
            {
                Id = updatedProduct.Id,
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                Categories = updatedProduct.ProductCategories.Select(pc => new CategoryDTO
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name
                }).ToList()
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting product with ID {Id}", id);

            var deleted = await _productRepository.DeleteProductAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
