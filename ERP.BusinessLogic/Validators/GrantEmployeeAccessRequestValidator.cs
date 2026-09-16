using FluentValidation;

public class GrantEmployeeAccessRequestValidator : AbstractValidator<GrantEmployeeAccessRequest>
{
    public GrantEmployeeAccessRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required");
        //RuleFor(x => x.RoleNames).NotEmpty().WithMessage("At least one role must be assigned");
    }
}