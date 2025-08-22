using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentResultGameAndIsWinner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Eliminated",
                table: "LobbyParticipants");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "LobbyParticipants");

            migrationBuilder.AddColumn<int>(
                name: "Game",
                table: "TournamentResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsWinner",
                table: "TournamentResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Game",
                table: "TournamentResults");

            migrationBuilder.DropColumn(
                name: "IsWinner",
                table: "TournamentResults");

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
    }
}
