using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrapCam.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Animals_ExternalId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Animals");

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CameraId = table.Column<Guid>(type: "uuid", nullable: false),
                    S3Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ImageDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Highlight = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageAnimals",
                columns: table => new
                {
                    AnimalsOnImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageAnimals", x => new { x.AnimalsOnImageId, x.ImageId });
                    table.ForeignKey(
                        name: "FK_ImageAnimals_Animals_AnimalsOnImageId",
                        column: x => x.AnimalsOnImageId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageAnimals_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageAnimals_ImageId",
                table: "ImageAnimals",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_CameraId",
                table: "Images",
                column: "CameraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageAnimals");

            migrationBuilder.DropTable(
                name: "Images");

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
    }
}
