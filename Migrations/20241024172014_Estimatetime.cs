using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YatriSewa.Migrations
{
    /// <inheritdoc />
    public partial class Estimatetime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EstimatedTime",
                table: "Route_Table",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EstimatedTime",
                table: "Route_Table",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
