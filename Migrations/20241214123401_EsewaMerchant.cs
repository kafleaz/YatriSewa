using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class EsewaMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EsewaTransaction_Table",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ServiceCharge = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ProductCharge = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BookingId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EsewaTransaction_Table", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "Merchant_Table",
                columns: table => new
                {
                    MerchantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    MerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ServiceCharge = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ProductCharge = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchant_Table", x => x.MerchantId);
                    table.ForeignKey(
                        name: "FK_Merchant_Table_Company_Table_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company_Table",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_Merchant_Table_Driver_Table_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver_Table",
                        principalColumn: "DriverId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Merchant_Table_CompanyId",
                table: "Merchant_Table",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchant_Table_DriverId",
                table: "Merchant_Table",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EsewaTransaction_Table");

            migrationBuilder.DropTable(
                name: "Merchant_Table");
        }
    }
}
