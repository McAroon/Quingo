using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class NodeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DeletedAt",
                table: "Tags",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_DeletedAt",
                table: "Packs",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PackPresets_DeletedAt",
                table: "PackPresets",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTags_DeletedAt",
                table: "NodeTags",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CreatedAt",
                table: "Nodes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_DeletedAt",
                table: "Nodes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ImageUrl",
                table: "Nodes",
                column: "ImageUrl");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Name",
                table: "Nodes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinkTypes_DeletedAt",
                table: "NodeLinkTypes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_DeletedAt",
                table: "NodeLinks",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinkSteps_DeletedAt",
                table: "IndirectLinkSteps",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IndirectLinks_DeletedAt",
                table: "IndirectLinks",
                column: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_DeletedAt",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Packs_DeletedAt",
                table: "Packs");

            migrationBuilder.DropIndex(
                name: "IX_PackPresets_DeletedAt",
                table: "PackPresets");

            migrationBuilder.DropIndex(
                name: "IX_NodeTags_DeletedAt",
                table: "NodeTags");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_CreatedAt",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_DeletedAt",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_ImageUrl",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_Name",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_NodeLinkTypes_DeletedAt",
                table: "NodeLinkTypes");

            migrationBuilder.DropIndex(
                name: "IX_NodeLinks_DeletedAt",
                table: "NodeLinks");

            migrationBuilder.DropIndex(
                name: "IX_IndirectLinkSteps_DeletedAt",
                table: "IndirectLinkSteps");

            migrationBuilder.DropIndex(
                name: "IX_IndirectLinks_DeletedAt",
                table: "IndirectLinks");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");
        }
    }
}
