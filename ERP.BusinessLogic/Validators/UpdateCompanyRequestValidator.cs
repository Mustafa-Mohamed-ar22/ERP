using FluentValidation;

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Company name is required").MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required").MaximumLength(10);
        RuleFor(x => x.LegalName).MaximumLength(200);
        RuleFor(x => x.TaxNumber).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}
