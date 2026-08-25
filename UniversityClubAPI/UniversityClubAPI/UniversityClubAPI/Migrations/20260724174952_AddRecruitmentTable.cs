using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClubApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubApplications_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubApplications_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubApplications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubApplications_AppliedAt",
                table: "ClubApplications",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClubApplications_ClubId_Status",
                table: "ClubApplications",
                columns: new[] { "ClubId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubApplications_ReviewedBy",
                table: "ClubApplications",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClubApplications_UserId_ClubId",
                table: "ClubApplications",
                columns: new[] { "UserId", "ClubId" },
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClubApplications_UserId_Status",
                table: "ClubApplications",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubApplications");
        }
    }
}
