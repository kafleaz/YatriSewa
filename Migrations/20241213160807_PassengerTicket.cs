using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class PassengerTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PNR",
                table: "Ticket_Table",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PassengerId",
                table: "Ticket_Table",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketNo",
                table: "Ticket_Table",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PassengerId",
                table: "Payment_Table",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassengerId",
                table: "Booking_Table",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Passenger_Table",
                columns: table => new
                {
                    PassengerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    BoardingPoint = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DroppingPoint = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passenger_Table", x => x.PassengerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Table_PassengerId",
                table: "Ticket_Table",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Table_PassengerId",
                table: "Payment_Table",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Table_PassengerId",
                table: "Booking_Table",
                column: "PassengerId",
                unique: true,
                filter: "[PassengerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Table_Passenger_Table_PassengerId",
                table: "Booking_Table",
                column: "PassengerId",
                principalTable: "Passenger_Table",
                principalColumn: "PassengerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Table_Passenger_Table_PassengerId",
                table: "Payment_Table",
                column: "PassengerId",
                principalTable: "Passenger_Table",
                principalColumn: "PassengerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Table_Passenger_Table_PassengerId",
                table: "Ticket_Table",
                column: "PassengerId",
                principalTable: "Passenger_Table",
                principalColumn: "PassengerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Table_Passenger_Table_PassengerId",
                table: "Booking_Table");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Table_Passenger_Table_PassengerId",
                table: "Payment_Table");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Table_Passenger_Table_PassengerId",
                table: "Ticket_Table");

            migrationBuilder.DropTable(
                name: "Passenger_Table");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_Table_PassengerId",
                table: "Ticket_Table");

            migrationBuilder.DropIndex(
                name: "IX_Payment_Table_PassengerId",
                table: "Payment_Table");

            migrationBuilder.DropIndex(
                name: "IX_Booking_Table_PassengerId",
                table: "Booking_Table");

            migrationBuilder.DropColumn(
                name: "PNR",
                table: "Ticket_Table");

            migrationBuilder.DropColumn(
                name: "PassengerId",
                table: "Ticket_Table");

            migrationBuilder.DropColumn(
                name: "TicketNo",
                table: "Ticket_Table");

            migrationBuilder.DropColumn(
                name: "PassengerId",
                table: "Payment_Table");

            migrationBuilder.DropColumn(
                name: "PassengerId",
                table: "Booking_Table");
        }
    }
}
