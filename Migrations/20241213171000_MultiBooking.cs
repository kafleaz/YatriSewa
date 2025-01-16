using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class MultiBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Table_PassengerId",
                table: "Booking_Table");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Table_PassengerId",
                table: "Booking_Table",
                column: "PassengerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Table_PassengerId",
                table: "Booking_Table");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Table_PassengerId",
                table: "Booking_Table",
                column: "PassengerId",
                unique: true,
                filter: "[PassengerId] IS NOT NULL");
        }
    }
}
