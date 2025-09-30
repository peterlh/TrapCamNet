using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrapCam.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailArchiveEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailArchives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CameraId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    S3Link = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FromEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FromName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    S3Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ImageS3Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ImageS3Link = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailArchives_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailArchives_CameraId",
                table: "EmailArchives",
                column: "CameraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailArchives");
        }
    }
}
