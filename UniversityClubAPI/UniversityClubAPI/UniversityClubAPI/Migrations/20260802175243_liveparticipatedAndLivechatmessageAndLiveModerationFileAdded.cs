using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class liveparticipatedAndLivechatmessageAndLiveModerationFileAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LiveEndedAt",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LiveStartedAt",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LiveStatus",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MeetingLink",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LiveChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveChatMessages_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveChatMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveModerations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsMuted = table.Column<bool>(type: "bit", nullable: false),
                    IsBanned = table.Column<bool>(type: "bit", nullable: false),
                    ModeratedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveModerations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveModerations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveModerations_Users_ModeratedBy",
                        column: x => x.ModeratedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveModerations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveParticipants_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiveParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveChatMessages_EventId_SentAt",
                table: "LiveChatMessages",
                columns: new[] { "EventId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveChatMessages_UserId",
                table: "LiveChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveModerations_EventId_UserId",
                table: "LiveModerations",
                columns: new[] { "EventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveModerations_ModeratedBy",
                table: "LiveModerations",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LiveModerations_UserId",
                table: "LiveModerations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveParticipants_EventId_LeftAt",
                table: "LiveParticipants",
                columns: new[] { "EventId", "LeftAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveParticipants_EventId_UserId",
                table: "LiveParticipants",
                columns: new[] { "EventId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveParticipants_UserId",
                table: "LiveParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveChatMessages");

            migrationBuilder.DropTable(
                name: "LiveModerations");

            migrationBuilder.DropTable(
                name: "LiveParticipants");

            migrationBuilder.DropColumn(
                name: "LiveEndedAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LiveStartedAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LiveStatus",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MeetingLink",
                table: "Events");
        }
    }
}
