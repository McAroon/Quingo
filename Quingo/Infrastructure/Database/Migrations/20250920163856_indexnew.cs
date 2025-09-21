using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class indexnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LobbyParticipants_TournamentLobbyId",
                table: "LobbyParticipants");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentResults_LobbyId_UpdatedAt_CreatedAt",
                table: "TournamentResults",
                columns: new[] { "LobbyId", "UpdatedAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentLobbies_UpdatedAt",
                table: "TournamentLobbies",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_TournamentLobbyId_CreatedAt",
                table: "LobbyParticipants",
                columns: new[] { "TournamentLobbyId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TournamentResults_LobbyId_UpdatedAt_CreatedAt",
                table: "TournamentResults");

            migrationBuilder.DropIndex(
                name: "IX_TournamentLobbies_UpdatedAt",
                table: "TournamentLobbies");

            migrationBuilder.DropIndex(
                name: "IX_LobbyParticipants_TournamentLobbyId_CreatedAt",
                table: "LobbyParticipants");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_TournamentLobbyId",
                table: "LobbyParticipants",
                column: "TournamentLobbyId");
        }
    }
}
