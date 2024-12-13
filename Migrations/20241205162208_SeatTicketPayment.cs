using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class SeatTicketPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Booking_Table",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: true),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking_Table", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Booking_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId");
                    table.ForeignKey(
                        name: "FK_Booking_Table_User_Table_UserId",
                        column: x => x.UserId,
                        principalTable: "User_Table",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payment_Table",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment_Table", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payment_Table_Booking_Table_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking_Table",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payment_Table_User_Table_UserId",
                        column: x => x.UserId,
                        principalTable: "User_Table",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Seat_Table",
                columns: table => new
                {
                    SeatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeatNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    ReservedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReservedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seat_Table", x => x.SeatId);
                    table.ForeignKey(
                        name: "FK_Seat_Table_Booking_Table_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking_Table",
                        principalColumn: "BookingId");
                    table.ForeignKey(
                        name: "FK_Seat_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seat_Table_User_Table_ReservedByUserId",
                        column: x => x.ReservedByUserId,
                        principalTable: "User_Table",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Ticket_Table",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    SeatId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket_Table", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Ticket_Table_Booking_Table_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Booking_Table",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ticket_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId");
                    table.ForeignKey(
                        name: "FK_Ticket_Table_Seat_Table_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seat_Table",
                        principalColumn: "SeatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Table_BusId",
                table: "Booking_Table",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Table_UserId",
                table: "Booking_Table",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Table_BookingId",
                table: "Payment_Table",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Table_UserId",
                table: "Payment_Table",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_Table_BookingId",
                table: "Seat_Table",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_Table_BusId",
                table: "Seat_Table",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_Table_ReservedByUserId",
                table: "Seat_Table",
                column: "ReservedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Table_BookingId",
                table: "Ticket_Table",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Table_BusId",
                table: "Ticket_Table",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Table_SeatId",
                table: "Ticket_Table",
                column: "SeatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payment_Table");

            migrationBuilder.DropTable(
                name: "Ticket_Table");

            migrationBuilder.DropTable(
                name: "Seat_Table");

            migrationBuilder.DropTable(
                name: "Booking_Table");
        }
    }
}
