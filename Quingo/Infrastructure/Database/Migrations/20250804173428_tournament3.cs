using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Quingo.Migrations
{
    /// <inheritdoc />
    public partial class tournament3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentLobbies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HostUserId = table.Column<string>(type: "text", nullable: false),
                    HostUserName = table.Column<string>(type: "text", nullable: false),
                    PackId = table.Column<int>(type: "integer", nullable: false),
                    PackName = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentLobbies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentLobbies_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentLobbies_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TournamentLobbies_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LobbyParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentLobbyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    IsReady = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LobbyParticipants_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyParticipants_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyParticipants_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyParticipants_TournamentLobbies_TournamentLobbyId",
                        column: x => x.TournamentLobbyId,
                        principalTable: "TournamentLobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_CreatedByUserId",
                table: "LobbyParticipants",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_DeletedAt",
                table: "LobbyParticipants",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_DeletedByUserId",
                table: "LobbyParticipants",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_TournamentLobbyId",
                table: "LobbyParticipants",
                column: "TournamentLobbyId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyParticipants_UpdatedByUserId",
                table: "LobbyParticipants",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentLobbies_CreatedByUserId",
                table: "TournamentLobbies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentLobbies_DeletedAt",
                table: "TournamentLobbies",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentLobbies_DeletedByUserId",
                table: "TournamentLobbies",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentLobbies_UpdatedByUserId",
                table: "TournamentLobbies",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LobbyParticipants");

            migrationBuilder.DropTable(
                name: "TournamentLobbies");
        }
    }
}
