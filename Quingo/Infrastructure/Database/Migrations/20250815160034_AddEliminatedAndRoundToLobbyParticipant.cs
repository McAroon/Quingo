using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class AddEliminatedAndRoundToLobbyParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Eliminated",
                table: "LobbyParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "LobbyParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eliminated",
                table: "LobbyParticipants");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "LobbyParticipants");
        }
    }
}
