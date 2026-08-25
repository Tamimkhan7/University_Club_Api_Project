using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityClubAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGroupMessageAndMessageResponseAndMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediaType",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "GroupMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedForEveryone",
                table: "GroupMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MediaType",
                table: "GroupMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "IsDeletedForEveryone",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "GroupMessages");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "GroupMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
