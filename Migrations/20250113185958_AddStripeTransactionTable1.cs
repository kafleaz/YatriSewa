using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeTransactionTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
               name: "StripeTransId",
                table: "Payment_Table",
               type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StripeTrans_Table",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StripeTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeTrans_Table", x => x.TransactionId);
                });

            migrationBuilder.CreateIndex(

             name: "IX_Payment_Table_StripeTransId",
              table: "Payment_Table",
            column: "StripeTransId");

            //migrationBuilder.CreateIndex(
            //   name: "IX_Payment_Table_StripeTransId",
            //   table: "Payment_Table",
            //   column: "StripeTransId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Table_StripeTrans_Table_StripeTransId",
                table: "Payment_Table",
                column: "StripeTransId",
                principalTable: "StripeTrans_Table",
                principalColumn: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Table_StripeTrans_Table_StripeTransId",
                table: "Payment_Table");

            migrationBuilder.DropTable(
                name: "StripeTrans_Table");

            migrationBuilder.DropIndex(
                name: "IX_Payment_Table_StripeTransId",
                table: "Payment_Table");

            migrationBuilder.DropColumn(
                name: "StripeTransId",
                table: "Payment_Table");
        }
    }
}
