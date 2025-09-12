using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPackPreset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPackPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPackPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPackPresets_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPackPresets_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPackPresets_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_CreatedByUserId",
                table: "UserPackPresets",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_DeletedAt",
                table: "UserPackPresets",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_DeletedByUserId",
                table: "UserPackPresets",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_UpdatedByUserId",
                table: "UserPackPresets",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_UserId_PackId",
                table: "UserPackPresets",
                columns: new[] { "UserId", "PackId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPackPresets");
        }
    }
}
