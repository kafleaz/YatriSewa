using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "User_Table",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "User_Table",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Company_Table",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContactInfo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company_Table", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Driver_Table",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicensePhotoPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Driver_Table", x => x.DriverId);
                    table.ForeignKey(
                        name: "FK_Driver_Table_User_Table_UserId",
                        column: x => x.UserId,
                        principalTable: "User_Table",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Route_Table",
                columns: table => new
                {
                    RouteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Stops = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EndLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EstimatedTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Route_Table", x => x.RouteID);
                    table.ForeignKey(
                        name: "FK_Route_Table_Company_Table_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Company_Table",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bus_Table",
                columns: table => new
                {
                    BusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SeatCapacity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bus_Table", x => x.BusId);
                    table.ForeignKey(
                        name: "FK_Bus_Table_Company_Table_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company_Table",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bus_Table_Driver_Table_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver_Table",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bus_Table_Route_Table_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Route_Table",
                        principalColumn: "RouteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Service_Table",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Wifi = table.Column<bool>(type: "bit", nullable: false),
                    AC = table.Column<bool>(type: "bit", nullable: false),
                    BusType = table.Column<int>(type: "int", nullable: false),
                    safety_features = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    essentials = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    snacks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service_Table", x => x.ServiceId);
                    table.ForeignKey(
                        name: "FK_Service_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Table_CompanyID",
                table: "User_Table",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Bus_Table_CompanyId",
                table: "Bus_Table",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Bus_Table_DriverId",
                table: "Bus_Table",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Bus_Table_RouteId",
                table: "Bus_Table",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_Table_UserId",
                table: "Driver_Table",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Route_Table_CompanyID",
                table: "Route_Table",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Service_Table_BusId",
                table: "Service_Table",
                column: "BusId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Table_Company_Table_CompanyID",
                table: "User_Table",
                column: "CompanyID",
                principalTable: "Company_Table",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Table_Company_Table_CompanyID",
                table: "User_Table");

            migrationBuilder.DropTable(
                name: "Service_Table");

            migrationBuilder.DropTable(
                name: "Bus_Table");

            migrationBuilder.DropTable(
                name: "Driver_Table");

            migrationBuilder.DropTable(
                name: "Route_Table");

            migrationBuilder.DropTable(
                name: "Company_Table");

            migrationBuilder.DropIndex(
                name: "IX_User_Table_CompanyID",
                table: "User_Table");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "User_Table");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "User_Table");
        }
    }
}
