using System.Collections.Generic;

namespace ProductCategoryAPI.DTOs
{
    public class CreateProductRequest
    {
        public ProductDTO ProductDTO { get; set; }
        public List<int> CategoryIds { get; set; }
    }
}
