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
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _productRepository.GetAllProductsAsync(page, pageSize);

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
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var productDTO = new ProductDTO
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
            };

            return Ok(productDTO);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.CategoryIds == null || request.CategoryIds.Count < 2 || request.CategoryIds.Count > 3)
                return BadRequest("A product must have 2 or 3 categories.");

            var product = new Product
            {
                Name = request.ProductDTO.Name,
                Description = request.ProductDTO.Description,
                Price = request.ProductDTO.Price
            };

            var newProduct = await _productRepository.AddProductAsync(product, request.CategoryIds);

            var responseDTO = new ProductDTO
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
            };

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, responseDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.ProductDTO.Id)
                return BadRequest("Product ID in URL and request body must match.");

            if (request.CategoryIds == null || request.CategoryIds.Count < 2 || request.CategoryIds.Count > 3)
                return BadRequest("A product must have 2 or 3 categories.");

            var product = new Product
            {
                Id = request.ProductDTO.Id,
                Name = request.ProductDTO.Name,
                Description = request.ProductDTO.Description,
                Price = request.ProductDTO.Price
            };

            var updatedProduct = await _productRepository.UpdateProductAsync(product, request.CategoryIds);
            if (updatedProduct == null) return NotFound();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productRepository.DeleteProductAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
