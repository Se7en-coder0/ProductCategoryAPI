using FluentValidation;
using ProductCategoryAPI.DTOs;

namespace ProductCategoryAPI.Validators
{
    public class CategoryDTOValidator : AbstractValidator<CategoryDTO>
    {
        public CategoryDTOValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must be less than 100 characters.");
        }
    }
}
