using FluentValidation;

public class AssignUserRolesRequestValidator : AbstractValidator<AssignUserRolesRequest>
{
    public AssignUserRolesRequestValidator()
    {
        RuleFor(x => x.RoleNames).NotEmpty().WithMessage("At least one role must be assigned");
    }
}