using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class PhoneNumberinfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WalkInCustomerPhone",
                table: "CashierOrders",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalkInCustomerPhone",
                table: "CashierInvoices",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalkInCustomerPhone",
                table: "CashierOrders");

            migrationBuilder.DropColumn(
                name: "WalkInCustomerPhone",
                table: "CashierInvoices");
        }
    }
}
