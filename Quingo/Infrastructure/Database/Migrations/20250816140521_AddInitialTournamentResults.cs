using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialTournamentResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "TournamentResults",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "TournamentResults");
        }
    }
}
