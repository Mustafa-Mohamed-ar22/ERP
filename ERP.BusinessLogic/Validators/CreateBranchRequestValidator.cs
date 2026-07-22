using FluentValidation;
public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Branch name is required").MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().WithMessage("Branch code is required").MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(30)
            .Matches(@"^(\+20|0020|0)?1[0125][0-9]{8}$")
                .WithMessage("Phone must be a valid Egyptian mobile number (e.g. 01012345678, +201012345678, or 00201012345678).");
    }
}
