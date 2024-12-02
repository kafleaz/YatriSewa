using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedule_Table",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusId = table.Column<int>(type: "int", nullable: true),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    BusCompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule_Table", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId");
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Company_Table_BusCompanyId",
                        column: x => x.BusCompanyId,
                        principalTable: "Company_Table",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Driver_Table_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver_Table",
                        principalColumn: "DriverId");
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Route_Table_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Route_Table",
                        principalColumn: "RouteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_Table_BusCompanyId",
                table: "Schedule_Table",
                column: "BusCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_Table_BusId",
                table: "Schedule_Table",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_Table_DriverId",
                table: "Schedule_Table",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_Table_RouteId",
                table: "Schedule_Table",
                column: "RouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedule_Table");
        }
    }
}
