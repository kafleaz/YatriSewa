using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class IotDevicesupdatebus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IoTDeviceLocationLogs",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Speed = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LocationDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsMoving = table.Column<bool>(type: "bit", nullable: false),
                    HasPassengers = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IoTDeviceLocationLogs", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_IoTDeviceLocationLogs_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IoTDevices",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    Speed = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IoTDevices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_IoTDevices_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId");
                });

            migrationBuilder.CreateTable(
                name: "PassengerLocationLogs",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PassengerId = table.Column<int>(type: "int", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassengerLocationLogs", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_PassengerLocationLogs_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PassengerLocationLogs_Passenger_Table_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Passenger_Table",
                        principalColumn: "PassengerId",
                        onDelete: ReferentialAction.Restrict); // Changed from Cascade to Restrict
                });

            migrationBuilder.CreateIndex(
                name: "IX_IoTDeviceLocationLogs_BusId",
                table: "IoTDeviceLocationLogs",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_BusId",
                table: "IoTDevices",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_PassengerLocationLogs_BusId",
                table: "PassengerLocationLogs",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_PassengerLocationLogs_PassengerId",
                table: "PassengerLocationLogs",
                column: "PassengerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IoTDeviceLocationLogs");

            migrationBuilder.DropTable(
                name: "IoTDevices");

            migrationBuilder.DropTable(
                name: "PassengerLocationLogs");
        }
    }
}
