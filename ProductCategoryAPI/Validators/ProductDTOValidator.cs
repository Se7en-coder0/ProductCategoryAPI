using FluentValidation;
using ProductCategoryAPI.DTOs;

namespace ProductCategoryAPI.Validators
{
    public class ProductDTOValidator : AbstractValidator<ProductDTO>
    {
        public ProductDTOValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must be less than 200 characters.");

            RuleFor(p => p.Description)
                .NotEmpty().WithMessage("Product description is required.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }
}
