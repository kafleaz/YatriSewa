using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteGeolocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EndLatitude",
                table: "Route_Table",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EndLongitude",
                table: "Route_Table",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartLatitude",
                table: "Route_Table",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartLongitude",
                table: "Route_Table",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndLatitude",
                table: "Route_Table");

            migrationBuilder.DropColumn(
                name: "EndLongitude",
                table: "Route_Table");

            migrationBuilder.DropColumn(
                name: "StartLatitude",
                table: "Route_Table");

            migrationBuilder.DropColumn(
                name: "StartLongitude",
                table: "Route_Table");
        }
    }
}
