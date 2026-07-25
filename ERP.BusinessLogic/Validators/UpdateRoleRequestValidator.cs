using FluentValidation;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required").MaximumLength(100);
    }
}
