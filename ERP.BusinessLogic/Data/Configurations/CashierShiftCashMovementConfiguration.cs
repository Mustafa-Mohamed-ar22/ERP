using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierShiftCashMovementConfiguration : IEntityTypeConfiguration<CashierShiftCashMovement>
{
    public void Configure(EntityTypeBuilder<CashierShiftCashMovement> builder)
    {
        builder.ToTable("CashierShiftCashMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MovementType).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Amount).HasColumnType("decimal(18,2)");
        builder.Property(m => m.Reason).IsRequired().HasMaxLength(250);
    }
}