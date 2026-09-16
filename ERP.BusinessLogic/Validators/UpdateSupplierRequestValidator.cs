using FluentValidation;

public class UpdateSupplierRequestValidator : AbstractValidator<UpdateSupplierRequest>
{
    public UpdateSupplierRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Supplier name is required").MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
    .Matches(@"^(?:\+20|0020|0)?1[0125]\d{8}$")
    .When(x => x.Phone != null)
    .WithMessage("Invalid Egyptian phone number format. Example: +201012345678 or 01012345678");
    }
}
