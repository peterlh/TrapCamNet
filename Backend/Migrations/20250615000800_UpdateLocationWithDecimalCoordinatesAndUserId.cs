using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrapCam.Backend.Migrations
{
    /// <summary>
    /// Migration to update Location entity with decimal coordinates and add UserId
    /// </summary>
    public partial class UpdateLocationWithDecimalCoordinatesAndUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Change Lat and Long from int to decimal
            migrationBuilder.AlterColumn<decimal>(
                name: "Long",
                table: "Locations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Lat",
                table: "Locations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            // Add UserId column
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Locations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove UserId column
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Locations");

            // Change Lat and Long back to int
            migrationBuilder.AlterColumn<int>(
                name: "Long",
                table: "Locations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Lat",
                table: "Locations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
