using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class IndirectLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndirectLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    TagFromId = table.Column<int>(type: "integer", nullable: true),
                    TagToId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndirectLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndirectLinks_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndirectLinks_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndirectLinks_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndirectLinks_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndirectLinks_Tags_TagFromId",
                        column: x => x.TagFromId,
                        principalTable: "Tags",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndirectLinks_Tags_TagToId",
                        column: x => x.TagToId,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_CreatedByUserId",
                table: "IndirectLinks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_DeletedByUserId",
                table: "IndirectLinks",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_PackId",
                table: "IndirectLinks",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_TagFromId",
                table: "IndirectLinks",
                column: "TagFromId");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_TagToId",
                table: "IndirectLinks",
                column: "TagToId");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_UpdatedByUserId",
                table: "IndirectLinks",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndirectLinks");
        }
    }
}
