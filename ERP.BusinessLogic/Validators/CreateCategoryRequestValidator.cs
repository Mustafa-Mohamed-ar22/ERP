using FluentValidation;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required").MaximumLength(150);
    }
}
