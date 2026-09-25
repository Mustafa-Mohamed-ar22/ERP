using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccountingSettingsConfiguration : IEntityTypeConfiguration<AccountingSettings>
{
    public void Configure(EntityTypeBuilder<AccountingSettings> builder)
    {
        builder.ToTable("AccountingSettings");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.CompanyId).IsUnique();

        builder.HasOne(s => s.InventoryAccount).WithMany().HasForeignKey(s => s.InventoryAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.AccountsPayableAccount).WithMany().HasForeignKey(s => s.AccountsPayableAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.AccountsReceivableAccount).WithMany().HasForeignKey(s => s.AccountsReceivableAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.RevenueAccount).WithMany().HasForeignKey(s => s.RevenueAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.CostOfGoodsSoldAccount).WithMany().HasForeignKey(s => s.CostOfGoodsSoldAccountId).OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(s => s.CashAccount).WithMany().HasForeignKey(s => s.CashAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}