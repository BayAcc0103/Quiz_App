using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzesForRoom_StudentQuizForRoomId",
                table: "StudentQuizQuestions");

            migrationBuilder.DropIndex(
                name: "IX_StudentQuizQuestions_StudentQuizForRoomId",
                table: "StudentQuizQuestions");

            migrationBuilder.DropColumn(
                name: "StudentQuizForRoomId",
                table: "StudentQuizQuestions");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Quizzes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELX2SS84huMMNukWZz35MTKxYBqFYqJUdWdrgJh7WXSYGln5sOn9CabofAgNxwuX0A==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Quizzes");

            migrationBuilder.AddColumn<int>(
                name: "StudentQuizForRoomId",
                table: "StudentQuizQuestions",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEL2Mk210XYkVWE8bSdXIKej1HZjjjeo7oN9kV00glg3sLXCE80Ur/fxs1prMiQjP0Q==");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizQuestions_StudentQuizForRoomId",
                table: "StudentQuizQuestions",
                column: "StudentQuizForRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzesForRoom_StudentQuizForRoomId",
                table: "StudentQuizQuestions",
                column: "StudentQuizForRoomId",
                principalTable: "StudentQuizzesForRoom",
                principalColumn: "Id");
        }
    }
}
