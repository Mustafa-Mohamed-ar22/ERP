using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class fixrecreateproblemgenerally : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_CompanyId_ProductId_WarehouseId",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_Settings_CompanyId_Key",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyId_Sku",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CompanyId_EntryNumber",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CompanyId_Code",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CompanyId_Code",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_CompanyId_ProductId_WarehouseId",
                table: "StockItems",
                columns: new[] { "CompanyId", "ProductId", "WarehouseId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_CompanyId_Key",
                table: "Settings",
                columns: new[] { "CompanyId", "Key" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_Sku",
                table: "Products",
                columns: new[] { "CompanyId", "Sku" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CompanyId_EntryNumber",
                table: "JournalEntries",
                columns: new[] { "CompanyId", "EntryNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId_Code",
                table: "Branches",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompanyId_Code",
                table: "Accounts",
                columns: new[] { "CompanyId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_CompanyId_ProductId_WarehouseId",
                table: "StockItems");

            migrationBuilder.DropIndex(
                name: "IX_Settings_CompanyId_Key",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyId_Sku",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CompanyId_EntryNumber",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CompanyId_Code",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CompanyId_Code",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_CompanyId_ProductId_WarehouseId",
                table: "StockItems",
                columns: new[] { "CompanyId", "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_CompanyId_Key",
                table: "Settings",
                columns: new[] { "CompanyId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_Sku",
                table: "Products",
                columns: new[] { "CompanyId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CompanyId_EntryNumber",
                table: "JournalEntries",
                columns: new[] { "CompanyId", "EntryNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId_Code",
                table: "Branches",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompanyId_Code",
                table: "Accounts",
                columns: new[] { "CompanyId", "Code" },
                unique: true);
        }
    }
}
