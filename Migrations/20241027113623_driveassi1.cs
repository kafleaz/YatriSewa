using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class driveassi1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Driver_Table",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Driver_Table",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAssigned",
                table: "Driver_Table",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DriverAssign_Table",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAssign_Table", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_DriverAssign_Table_Bus_Table_BusId",
                        column: x => x.BusId,
                        principalTable: "Bus_Table",
                        principalColumn: "BusId");
                    table.ForeignKey(
                        name: "FK_DriverAssign_Table_Driver_Table_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver_Table",
                        principalColumn: "DriverId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Driver_Table_CompanyId",
                table: "Driver_Table",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAssign_Table_BusId",
                table: "DriverAssign_Table",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverAssign_Table_DriverId",
                table: "DriverAssign_Table",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Driver_Table_Company_Table_CompanyId",
                table: "Driver_Table",
                column: "CompanyId",
                principalTable: "Company_Table",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Driver_Table_Company_Table_CompanyId",
                table: "Driver_Table");

            migrationBuilder.DropTable(
                name: "DriverAssign_Table");

            migrationBuilder.DropIndex(
                name: "IX_Driver_Table_CompanyId",
                table: "Driver_Table");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Driver_Table");

            migrationBuilder.DropColumn(
                name: "IsAssigned",
                table: "Driver_Table");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Driver_Table",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);
        }
    }
}
