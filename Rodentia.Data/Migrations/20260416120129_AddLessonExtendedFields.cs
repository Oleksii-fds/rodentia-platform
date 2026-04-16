using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rodentia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Homework",
                table: "lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaterialLinks",
                table: "lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressNote",
                table: "lessons",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Homework",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "MaterialLinks",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "ProgressNote",
                table: "lessons");
        }
    }
}
