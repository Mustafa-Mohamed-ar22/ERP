using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingSettingsAndGLIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountsPayableAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountsReceivableAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevenueAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CostOfGoodsSoldAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingSettings_Accounts_AccountsPayableAccountId",
                        column: x => x.AccountsPayableAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingSettings_Accounts_AccountsReceivableAccountId",
                        column: x => x.AccountsReceivableAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingSettings_Accounts_CostOfGoodsSoldAccountId",
                        column: x => x.CostOfGoodsSoldAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingSettings_Accounts_InventoryAccountId",
                        column: x => x.InventoryAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingSettings_Accounts_RevenueAccountId",
                        column: x => x.RevenueAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_AccountsPayableAccountId",
                table: "AccountingSettings",
                column: "AccountsPayableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_AccountsReceivableAccountId",
                table: "AccountingSettings",
                column: "AccountsReceivableAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_CompanyId",
                table: "AccountingSettings",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_CostOfGoodsSoldAccountId",
                table: "AccountingSettings",
                column: "CostOfGoodsSoldAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_InventoryAccountId",
                table: "AccountingSettings",
                column: "InventoryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_RevenueAccountId",
                table: "AccountingSettings",
                column: "RevenueAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingSettings");
        }
    }
}
