using FluentValidation;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required").MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Invalid email format");
        RuleFor(x => x.RoleNames).NotEmpty().WithMessage("At least one role must be assigned");
    }
}
