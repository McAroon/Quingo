using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class NodeLinkFromTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Nodes_Node1Id",
                table: "NodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Nodes_Node2Id",
                table: "NodeLinks");

            migrationBuilder.RenameColumn(
                name: "Node2Id",
                table: "NodeLinks",
                newName: "NodeToId");

            migrationBuilder.RenameColumn(
                name: "Node1Id",
                table: "NodeLinks",
                newName: "NodeFromId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeLinks_Node2Id",
                table: "NodeLinks",
                newName: "IX_NodeLinks_NodeToId");

            migrationBuilder.RenameIndex(
                name: "IX_NodeLinks_Node1Id",
                table: "NodeLinks",
                newName: "IX_NodeLinks_NodeFromId");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Nodes_NodeFromId",
                table: "NodeLinks",
                column: "NodeFromId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Nodes_NodeToId",
                table: "NodeLinks",
                column: "NodeToId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Nodes_NodeFromId",
                table: "NodeLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeLinks_Nodes_NodeToId",
                table: "NodeLinks");

            migrationBuilder.RenameColumn(
                name: "NodeToId",
                table: "NodeLinks",
                newName: "Node2Id");

            migrationBuilder.RenameColumn(
                name: "NodeFromId",
                table: "NodeLinks",
                newName: "Node1Id");

            migrationBuilder.RenameIndex(
                name: "IX_NodeLinks_NodeToId",
                table: "NodeLinks",
                newName: "IX_NodeLinks_Node2Id");

            migrationBuilder.RenameIndex(
                name: "IX_NodeLinks_NodeFromId",
                table: "NodeLinks",
                newName: "IX_NodeLinks_Node1Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Nodes_Node1Id",
                table: "NodeLinks",
                column: "Node1Id",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeLinks_Nodes_Node2Id",
                table: "NodeLinks",
                column: "Node2Id",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
