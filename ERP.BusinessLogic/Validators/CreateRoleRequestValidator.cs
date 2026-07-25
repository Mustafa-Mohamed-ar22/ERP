using FluentValidation;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required").MaximumLength(100);
        RuleFor(x => x.PermissionCodes).NotEmpty().WithMessage("At least one permission must be selected");
    }
}
