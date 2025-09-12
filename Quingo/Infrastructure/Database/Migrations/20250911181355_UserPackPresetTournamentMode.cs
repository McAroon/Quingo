using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class UserPackPresetTournamentMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPackPresets_UserId_PackId",
                table: "UserPackPresets");

            migrationBuilder.AddColumn<int>(
                name: "TournamentMode",
                table: "UserPackPresets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_UserId_PackId_TournamentMode",
                table: "UserPackPresets",
                columns: new[] { "UserId", "PackId", "TournamentMode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPackPresets_UserId_PackId_TournamentMode",
                table: "UserPackPresets");

            migrationBuilder.DropColumn(
                name: "TournamentMode",
                table: "UserPackPresets");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackPresets_UserId_PackId",
                table: "UserPackPresets",
                columns: new[] { "UserId", "PackId" },
                unique: true);
        }
    }
}
