using FluentValidation;

public class CreateLeaveRequestRequestValidator : AbstractValidator<CreateLeaveRequestRequest>
{
    public CreateLeaveRequestRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEqual(default(DateTime)).WithMessage("Start date is required");
        RuleFor(x => x.EndDate).NotEqual(default(DateTime)).WithMessage("End date is required");
        RuleFor(x => x.LeaveType).Must(t => Enum.TryParse<LeaveType>(t, true, out _))
            .WithMessage("Leave type must be one of: Annual, Sick, Unpaid, Other");
    }
}