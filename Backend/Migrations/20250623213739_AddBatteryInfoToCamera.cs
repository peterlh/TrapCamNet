using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrapCam.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBatteryInfoToCamera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatteryInfoJson",
                table: "Cameras",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatteryInfoJson",
                table: "Cameras");
        }
    }
}
