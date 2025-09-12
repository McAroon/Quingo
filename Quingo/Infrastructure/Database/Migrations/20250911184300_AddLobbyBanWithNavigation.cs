using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class AddLobbyBanWithNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LobbyBans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentLobbyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LobbyBans_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyBans_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyBans_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyBans_TournamentLobbies_TournamentLobbyId",
                        column: x => x.TournamentLobbyId,
                        principalTable: "TournamentLobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LobbyBans_CreatedByUserId",
                table: "LobbyBans",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyBans_DeletedAt",
                table: "LobbyBans",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyBans_DeletedByUserId",
                table: "LobbyBans",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyBans_TournamentLobbyId_UserId",
                table: "LobbyBans",
                columns: new[] { "TournamentLobbyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LobbyBans_UpdatedByUserId",
                table: "LobbyBans",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LobbyBans");
        }
    }
}
