using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class NodeLinkType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "PackId",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NodeLinkTypeId",
                table: "NodeLinks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NodeLinkTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeLinkTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeLinkTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeLinkTypes_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeLinkTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeLinkTypes_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_PackId",
                table: "Tags",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_NodeLinkTypeId",
                table: "NodeLinks",
                column: "NodeLinkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinkTypes_CreatedByUserId",
                table: "NodeLinkTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinkTypes_DeletedByUserId",
                table: "NodeLinkTypes",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinkTypes_PackId",
                table: "NodeLinkTypes",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinkTypes_UpdatedByUserId",
                table: "NodeLinkTypes",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_NodeLinkTypes_NodeLinkTypeId",
                table: "NodeLinks",
                column: "NodeLinkTypeId",
                principalTable: "NodeLinkTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Packs_PackId",
                table: "Tags",
                column: "PackId",
                principalTable: "Packs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_NodeLinkTypes_NodeLinkTypeId",
                table: "NodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Packs_PackId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "NodeLinkTypes");

            migrationBuilder.DropIndex(
                name: "IX_Tags_PackId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_NodeLinks_NodeLinkTypeId",
                table: "NodeLinks");

            migrationBuilder.DropColumn(
                name: "PackId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "NodeLinkTypeId",
                table: "NodeLinks");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
