using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class TournamentLobbyPresetJsonToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresetJson",
                table: "TournamentLobbies");

            migrationBuilder.AddColumn<string>(
                name: "PresetData",
                table: "TournamentLobbies",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresetData",
                table: "TournamentLobbies");

            migrationBuilder.AddColumn<string>(
                name: "PresetJson",
                table: "TournamentLobbies",
                type: "text",
                nullable: true);
        }
    }
}
