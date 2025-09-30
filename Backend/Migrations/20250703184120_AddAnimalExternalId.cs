using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrapCam.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Animals",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Animals_ExternalId",
                table: "Animals",
                column: "ExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Animals_ExternalId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Animals");
        }
    }
}
