using System.ComponentModel.DataAnnotations;

namespace ProductCategoryAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        //[Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
