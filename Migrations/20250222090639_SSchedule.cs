using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class SSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Table_CompanyID",
                table: "User_Table");

            //migrationBuilder.AddColumn<int>(
            //    name: "BusId",
            //    table: "User_Table",
            //    type: "int",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "BusId",
            //    table: "Passenger_Table",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_Table_CompanyID",
                table: "User_Table",
                column: "CompanyID",
                unique: true,
                filter: "[CompanyID] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Passenger_Table_BusId",
            //    table: "Passenger_Table",
            //    column: "BusId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Passenger_Table_Bus_Table_BusId",
            //    table: "Passenger_Table",
            //    column: "BusId",
            //    principalTable: "Bus_Table",
            //    principalColumn: "BusId",
            //    onDelete: ReferentialAction.Cascade);

            // Adding Schedule_Table
            migrationBuilder.CreateTable(
                name: "Schedule_Table",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusId = table.Column<int>(nullable: true),
                    RouteId = table.Column<int>(nullable: false),
                    DepartureTime = table.Column<DateTime>(nullable: false),
                    ArrivalTime = table.Column<DateTime>(nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AvailableSeats = table.Column<int>(nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    DriverId = table.Column<int>(nullable: true),
                    BusCompanyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule_Table", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Company_Table_BusCompanyId",
                        column: x => x.BusCompanyId,
                        principalTable: "Company_Table",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schedule_Table_Driver_Table_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver_Table",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Passenger_Table_Bus_Table_BusId",
                table: "Passenger_Table");

            migrationBuilder.DropIndex(
                name: "IX_User_Table_CompanyID",
                table: "User_Table");

            migrationBuilder.DropIndex(
                name: "IX_Passenger_Table_BusId",
                table: "Passenger_Table");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "User_Table");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "Passenger_Table");

            migrationBuilder.CreateIndex(
                name: "IX_User_Table_CompanyID",
                table: "User_Table",
                column: "CompanyID");

            // Removing Schedule_Table
            migrationBuilder.DropTable(
                name: "Schedule_Table");
        }
    }
}
