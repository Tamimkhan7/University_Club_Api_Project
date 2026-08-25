using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class addedClubPrivacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Clubs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ClubInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    InvitedUserId = table.Column<int>(type: "int", nullable: false),
                    InvitedBy = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubInvites_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubInvites_Users_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubInvites_Users_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubInvites_ClubId_InvitedUserId",
                table: "ClubInvites",
                columns: new[] { "ClubId", "InvitedUserId" },
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClubInvites_ClubId_InvitedUserId_Status",
                table: "ClubInvites",
                columns: new[] { "ClubId", "InvitedUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubInvites_InvitedBy",
                table: "ClubInvites",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClubInvites_InvitedUserId",
                table: "ClubInvites",
                column: "InvitedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubInvites");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Clubs");
        }
    }
}
