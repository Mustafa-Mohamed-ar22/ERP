using FluentValidation;

public class CreateJournalEntryRequestValidator : AbstractValidator<CreateJournalEntryRequest>
{
    public CreateJournalEntryRequestValidator()
    {
        RuleFor(x => x.EntryDate).NotEqual(default(DateTime)).WithMessage("Entry date is required");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least two lines are required")
            .Must(lines => lines.Count >= 2).WithMessage("A journal entry must have at least two lines");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Debit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Credit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l)
                .Must(l => (l.Debit > 0) ^ (l.Credit > 0))
                .WithMessage("Each line must have either a debit or a credit amount, not both or neither");
        });

        RuleFor(x => x.Lines)
            .Must(lines => lines.Sum(l => l.Debit) == lines.Sum(l => l.Credit))
            .WithMessage("Total debit must equal total credit")
            .When(x => x.Lines is { Count: > 0 });
    }
}