using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class indexed_names_phones_emails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_CompanyId_Name",
                table: "Warehouses",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CompanyId_Email",
                table: "Suppliers",
                columns: new[] { "CompanyId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CompanyId_Phone",
                table: "Suppliers",
                columns: new[] { "CompanyId", "Phone" },
                unique: true,
                filter: "[Phone] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CompanyId_Name",
                table: "Products",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

           

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CompanyId_Name",
                table: "ProductCategories",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyId_Email",
                table: "Employees",
                columns: new[] { "CompanyId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyId_Phone",
                table: "Employees",
                columns: new[] { "CompanyId", "Phone" },
                unique: true,
                filter: "[Phone] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId_Name",
                table: "Departments",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId_Email",
                table: "Customers",
                columns: new[] { "CompanyId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId_Phone",
                table: "Customers",
                columns: new[] { "CompanyId", "Phone" },
                unique: true,
                filter: "[Phone] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId_Name",
                table: "Branches",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId_Phone",
                table: "Branches",
                columns: new[] { "CompanyId", "Phone" },
                unique: true,
                filter: "[Phone] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompanyId_Name",
                table: "Accounts",
                columns: new[] { "CompanyId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouses_CompanyId_Name",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CompanyId_Email",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_CompanyId_Phone",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Products_CompanyId_Name",
                table: "Products");

          
            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_CompanyId_Name",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CompanyId_Email",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CompanyId_Phone",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CompanyId_Name",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CompanyId_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CompanyId_Phone",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Name",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CompanyId_Name",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CompanyId_Phone",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CompanyId_Name",
                table: "Accounts");
        }
    }
}