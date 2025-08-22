using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class AddIsClosedToTournamentLobby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "TournamentLobbies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "TournamentLobbies");
        }
    }
}
