using FluentValidation;

public class AssignRolePermissionsRequestValidator : AbstractValidator<AssignRolePermissionsRequest>
{
    public AssignRolePermissionsRequestValidator()
    {
        RuleFor(x => x.PermissionCodes).NotEmpty().WithMessage("At least one permission must be selected");
    }
}