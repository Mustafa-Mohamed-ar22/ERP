using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class CahierInvoiceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CashAccountId",
                table: "AccountingSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CashierShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpeningCashBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedClosingCash = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CountedClosingCash = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscrepancyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpenNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CloseNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierShifts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashierOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CashierShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalkInCustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangeDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VoidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoidedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VoidReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierOrders_CashierShifts_CashierShiftId",
                        column: x => x.CashierShiftId,
                        principalTable: "CashierShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashierOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashierOrders_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashierOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashierShiftCashMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierShiftCashMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierShiftCashMovements_CashierShifts_CashierShiftId",
                        column: x => x.CashierShiftId,
                        principalTable: "CashierShifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashierInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CashierOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WalkInCustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangeDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierInvoices_CashierOrders_CashierOrderId",
                        column: x => x.CashierOrderId,
                        principalTable: "CashierOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashierInvoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashierOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierOrderLines_CashierOrders_CashierOrderId",
                        column: x => x.CashierOrderId,
                        principalTable: "CashierOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashierOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashierPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierPayments_CashierOrders_CashierOrderId",
                        column: x => x.CashierOrderId,
                        principalTable: "CashierOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashierInvoiceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SkuSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierInvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierInvoiceLines_CashierInvoices_CashierInvoiceId",
                        column: x => x.CashierInvoiceId,
                        principalTable: "CashierInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_CashAccountId",
                table: "AccountingSettings",
                column: "CashAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInvoiceLines_CashierInvoiceId",
                table: "CashierInvoiceLines",
                column: "CashierInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInvoices_CashierOrderId",
                table: "CashierInvoices",
                column: "CashierOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashierInvoices_CompanyId_InvoiceNumber",
                table: "CashierInvoices",
                columns: new[] { "CompanyId", "InvoiceNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInvoices_CustomerId",
                table: "CashierInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrderLines_CashierOrderId",
                table: "CashierOrderLines",
                column: "CashierOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrderLines_ProductId",
                table: "CashierOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrders_CashierShiftId",
                table: "CashierOrders",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrders_CompanyId_OrderNumber",
                table: "CashierOrders",
                columns: new[] { "CompanyId", "OrderNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrders_CustomerId",
                table: "CashierOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrders_JournalEntryId",
                table: "CashierOrders",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierOrders_WarehouseId",
                table: "CashierOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierPayments_CashierOrderId",
                table: "CashierPayments",
                column: "CashierOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShiftCashMovements_CashierShiftId",
                table: "CashierShiftCashMovements",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_CashierUserId",
                table: "CashierShifts",
                column: "CashierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_CompanyId_ShiftNumber",
                table: "CashierShifts",
                columns: new[] { "CompanyId", "ShiftNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_CompanyId_WarehouseId_CashierUserId",
                table: "CashierShifts",
                columns: new[] { "CompanyId", "WarehouseId", "CashierUserId" },
                unique: true,
                filter: "[Status] = N'Open'");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_WarehouseId",
                table: "CashierShifts",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingSettings_Accounts_CashAccountId",
                table: "AccountingSettings",
                column: "CashAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingSettings_Accounts_CashAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropTable(
                name: "CashierInvoiceLines");

            migrationBuilder.DropTable(
                name: "CashierOrderLines");

            migrationBuilder.DropTable(
                name: "CashierPayments");

            migrationBuilder.DropTable(
                name: "CashierShiftCashMovements");

            migrationBuilder.DropTable(
                name: "CashierInvoices");

            migrationBuilder.DropTable(
                name: "CashierOrders");

            migrationBuilder.DropTable(
                name: "CashierShifts");

            migrationBuilder.DropIndex(
                name: "IX_AccountingSettings_CashAccountId",
                table: "AccountingSettings");

            migrationBuilder.DropColumn(
                name: "CashAccountId",
                table: "AccountingSettings");
        }
    }
}
