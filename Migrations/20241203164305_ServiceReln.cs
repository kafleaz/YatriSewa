using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class ServiceReln : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Service_Table_Bus_Table_BusId",
                table: "Service_Table");

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Table_Bus_Table_BusId",
                table: "Service_Table",
                column: "BusId",
                principalTable: "Bus_Table",
                principalColumn: "BusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Service_Table_Bus_Table_BusId",
                table: "Service_Table");

            migrationBuilder.AddForeignKey(
                name: "FK_Service_Table_Bus_Table_BusId",
                table: "Service_Table",
                column: "BusId",
                principalTable: "Bus_Table",
                principalColumn: "BusId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
