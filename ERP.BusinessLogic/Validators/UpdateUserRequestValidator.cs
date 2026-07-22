using FluentValidation;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required").MaximumLength(150);
    }
}
