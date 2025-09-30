using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrapCam.Backend.Migrations
{
    /// <summary>
    /// Migration to remove PublicCamera field from Camera entity and S3Link fields from EmailArchive entity
    /// </summary>
    public partial class RemovePublicCameraAndS3Links : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove PublicCamera column from Cameras table
            migrationBuilder.DropColumn(
                name: "PublicCamera",
                table: "Cameras");

            // Remove S3Link column from EmailArchives table
            migrationBuilder.DropColumn(
                name: "S3Link",
                table: "EmailArchives");

            // Remove ImageS3Link column from EmailArchives table
            migrationBuilder.DropColumn(
                name: "ImageS3Link",
                table: "EmailArchives");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add PublicCamera column back to Cameras table
            migrationBuilder.AddColumn<bool>(
                name: "PublicCamera",
                table: "Cameras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Add S3Link column back to EmailArchives table
            migrationBuilder.AddColumn<string>(
                name: "S3Link",
                table: "EmailArchives",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            // Add ImageS3Link column back to EmailArchives table
            migrationBuilder.AddColumn<string>(
                name: "ImageS3Link",
                table: "EmailArchives",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }
    }
}
