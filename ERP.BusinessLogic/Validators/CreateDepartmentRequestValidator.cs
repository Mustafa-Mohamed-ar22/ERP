using FluentValidation;

public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Department name is required").MaximumLength(150);
    }
}
