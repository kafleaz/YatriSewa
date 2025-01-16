using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class EsewaTransid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionId",
                table: "Payment_Table",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Table_TransactionId",
                table: "Payment_Table",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Table_EsewaTransaction_Table_TransactionId",
                table: "Payment_Table",
                column: "TransactionId",
                principalTable: "EsewaTransaction_Table",
                principalColumn: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Table_EsewaTransaction_Table_TransactionId",
                table: "Payment_Table");

            migrationBuilder.DropIndex(
                name: "IX_Payment_Table_TransactionId",
                table: "Payment_Table");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Payment_Table");
        }
    }
}
