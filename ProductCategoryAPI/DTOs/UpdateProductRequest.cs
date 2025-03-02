namespace ProductCategoryAPI.DTOs
{
    public class UpdateProductRequest
    {
        public ProductDTO ProductDTO { get; set; }
        public List<int> CategoryIds { get; set; }
    }
}
