using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class EntityUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Packs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Packs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Packs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Packs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Packs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "NodeTags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "NodeTags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "NodeTags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Nodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Nodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Nodes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "NodeLinks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "NodeLinks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "NodeLinks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CreatedByUserId",
                table: "Tags",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DeletedByUserId",
                table: "Tags",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UpdatedByUserId",
                table: "Tags",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_CreatedByUserId",
                table: "Packs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_DeletedByUserId",
                table: "Packs",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_UpdatedByUserId",
                table: "Packs",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTags_CreatedByUserId",
                table: "NodeTags",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTags_DeletedByUserId",
                table: "NodeTags",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTags_UpdatedByUserId",
                table: "NodeTags",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CreatedByUserId",
                table: "Nodes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_DeletedByUserId",
                table: "Nodes",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_UpdatedByUserId",
                table: "Nodes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_CreatedByUserId",
                table: "NodeLinks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_DeletedByUserId",
                table: "NodeLinks",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeLinks_UpdatedByUserId",
                table: "NodeLinks",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_AspNetUsers_CreatedByUserId",
                table: "NodeLinks",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_AspNetUsers_DeletedByUserId",
                table: "NodeLinks",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_AspNetUsers_UpdatedByUserId",
                table: "NodeLinks",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_AspNetUsers_CreatedByUserId",
                table: "Nodes",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_AspNetUsers_DeletedByUserId",
                table: "Nodes",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_AspNetUsers_UpdatedByUserId",
                table: "Nodes",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeTags_AspNetUsers_CreatedByUserId",
                table: "NodeTags",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeTags_AspNetUsers_DeletedByUserId",
                table: "NodeTags",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeTags_AspNetUsers_UpdatedByUserId",
                table: "NodeTags",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packs_AspNetUsers_CreatedByUserId",
                table: "Packs",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packs_AspNetUsers_DeletedByUserId",
                table: "Packs",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packs_AspNetUsers_UpdatedByUserId",
                table: "Packs",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_AspNetUsers_CreatedByUserId",
                table: "Tags",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_AspNetUsers_DeletedByUserId",
                table: "Tags",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_AspNetUsers_UpdatedByUserId",
                table: "Tags",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_AspNetUsers_CreatedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_AspNetUsers_DeletedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_AspNetUsers_UpdatedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_AspNetUsers_CreatedByUserId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_AspNetUsers_DeletedByUserId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_AspNetUsers_UpdatedByUserId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeTags_AspNetUsers_CreatedByUserId",
                table: "NodeTags");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeTags_AspNetUsers_DeletedByUserId",
                table: "NodeTags");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeTags_AspNetUsers_UpdatedByUserId",
                table: "NodeTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Packs_AspNetUsers_CreatedByUserId",
                table: "Packs");

            migrationBuilder.DropForeignKey(
                name: "FK_Packs_AspNetUsers_DeletedByUserId",
                table: "Packs");

            migrationBuilder.DropForeignKey(
                name: "FK_Packs_AspNetUsers_UpdatedByUserId",
                table: "Packs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_AspNetUsers_CreatedByUserId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_AspNetUsers_DeletedByUserId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_AspNetUsers_UpdatedByUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_CreatedByUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_DeletedByUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UpdatedByUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Packs_CreatedByUserId",
                table: "Packs");

            migrationBuilder.DropIndex(
                name: "IX_Packs_DeletedByUserId",
                table: "Packs");

            migrationBuilder.DropIndex(
                name: "IX_Packs_UpdatedByUserId",
                table: "Packs");

            migrationBuilder.DropIndex(
                name: "IX_NodeTags_CreatedByUserId",
                table: "NodeTags");

            migrationBuilder.DropIndex(
                name: "IX_NodeTags_DeletedByUserId",
                table: "NodeTags");

            migrationBuilder.DropIndex(
                name: "IX_NodeTags_UpdatedByUserId",
                table: "NodeTags");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_CreatedByUserId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_DeletedByUserId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_Nodes_UpdatedByUserId",
                table: "Nodes");

            migrationBuilder.DropIndex(
                name: "IX_NodeLinks_CreatedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropIndex(
                name: "IX_NodeLinks_DeletedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropIndex(
                name: "IX_NodeLinks_UpdatedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Packs");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Packs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Packs");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Packs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Packs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NodeTags");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "NodeTags");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "NodeTags");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Nodes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "NodeLinks");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "NodeLinks");
        }
    }
}
