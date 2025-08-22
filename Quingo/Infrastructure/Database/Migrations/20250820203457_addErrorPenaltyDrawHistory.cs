using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class addErrorPenaltyDrawHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "TournamentLobbies");

            migrationBuilder.AddColumn<int>(
                name: "DrawHistory",
                table: "TournamentResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ErrorPenalty",
                table: "TournamentResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrawHistory",
                table: "TournamentResults");

            migrationBuilder.DropColumn(
                name: "ErrorPenalty",
                table: "TournamentResults");

            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                table: "TournamentLobbies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
