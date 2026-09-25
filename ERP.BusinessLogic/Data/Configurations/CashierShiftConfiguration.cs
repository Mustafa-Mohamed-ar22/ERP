using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("CashierShifts");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ShiftNumber).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.OpeningCashBalance).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ExpectedClosingCash).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CountedClosingCash).HasColumnType("decimal(18,2)");
        builder.Property(s => s.DiscrepancyAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.OpenNotes).HasMaxLength(500);
        builder.Property(s => s.CloseNotes).HasMaxLength(500);
        builder.HasIndex(s => new { s.CompanyId, s.ShiftNumber }).IsUnique().HasSoftDeleteFilter();
        builder.HasIndex(s => s.CashierUserId); // no FK — deliberately unenforced

        // One open shift per cashier per warehouse — partial unique index
        builder.HasIndex(s => new { s.CompanyId, s.WarehouseId, s.CashierUserId })
            .HasFilter("[Status] = N'Open'")
            .IsUnique();

        builder.HasOne(s => s.Warehouse).WithMany().HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.Orders).WithOne(o => o.CashierShift).HasForeignKey(o => o.CashierShiftId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.CashMovements).WithOne(m => m.CashierShift).HasForeignKey(m => m.CashierShiftId).OnDelete(DeleteBehavior.Cascade);
    }
}