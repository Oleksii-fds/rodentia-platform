using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rodentia.Data.Migrations
{
    /// <inheritdoc />
    public partial class RodentiaFullDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Lessons");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payments");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "lessons");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lessons",
                table: "lessons",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_LessonId",
                table: "payments",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_lessons_StudentId",
                table: "lessons",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_lessons_TeacherId",
                table: "lessons",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_lessons_users_StudentId",
                table: "lessons",
                column: "StudentId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lessons_users_TeacherId",
                table: "lessons",
                column: "TeacherId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_lessons_LessonId",
                table: "payments",
                column: "LessonId",
                principalTable: "lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lessons_users_StudentId",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_lessons_users_TeacherId",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_lessons_LessonId",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_LessonId",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lessons",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "IX_lessons_StudentId",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "IX_lessons_TeacherId",
                table: "lessons");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "lessons",
                newName: "Lessons");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "Lessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons",
                column: "Id");
        }
    }
}
