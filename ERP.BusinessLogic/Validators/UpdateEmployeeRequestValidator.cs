using FluentValidation;

public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required").MaximumLength(150);
        RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Job title is required").MaximumLength(100);
        RuleFor(x => x.BaseSalary).NotEmpty().GreaterThanOrEqualTo(0);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Invalid email format");
        RuleFor(x => x.Status).Must(s => Enum.TryParse<EmploymentStatus>(s, true, out _))
            .WithMessage("Status must be one of: Active, OnLeave, Terminated");
        RuleFor(x => x.NationalId)
    .Matches(@"^[23]\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{2}\d{4}\d$")
    .When(x => !string.IsNullOrWhiteSpace(x.NationalId))
    .WithMessage("Invalid Egyptian National ID format.");


        RuleFor(x => x.Phone)
    .Matches(@"^(?:\+20|0020|0)?1[0125]\d{8}$")
    .When(x => x.Phone != null)
    .WithMessage("Invalid Egyptian phone number format. Example: +201012345678 or 01012345678");
    }
}
