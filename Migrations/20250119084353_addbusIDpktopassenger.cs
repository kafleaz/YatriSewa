using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class addbusIDpktopassenger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusId",
                table: "Passenger_Table",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Passenger_Table_BusId",
                table: "Passenger_Table",
                column: "BusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Passenger_Table_Bus_Table_BusId",
                table: "Passenger_Table",
                column: "BusId",
                principalTable: "Bus_Table",
                principalColumn: "BusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Passenger_Table_Bus_Table_BusId",
                table: "Passenger_Table");

            migrationBuilder.DropIndex(
                name: "IX_Passenger_Table_BusId",
                table: "Passenger_Table");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "Passenger_Table");
        }
    }
}
