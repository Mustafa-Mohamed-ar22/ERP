using FluentValidation;
public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Branch name is required").MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().WithMessage("Branch code is required").MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.Phone)
            .Matches(@"^(?:\+20|0020|0)?1[0125]\d{8}$")
            .When(x => x.Phone != null)
            .WithMessage("Invalid Egyptian phone number format. Example: +201012345678 or 01012345678");
    }
}
