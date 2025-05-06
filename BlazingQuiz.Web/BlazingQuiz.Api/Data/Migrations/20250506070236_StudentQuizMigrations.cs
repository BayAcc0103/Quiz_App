using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentQuizMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizQuestions_Questions_QuestionId",
                table: "StudentQuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzes_StudentQuizId",
                table: "StudentQuizQuestions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEMOPIDUxdY/9zyZ/PbDhQMwwcsELf3bniWC3XCTrwujd63+h3ZH7FgqwArEatCws/A==");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizQuestions_Questions_QuestionId",
                table: "StudentQuizQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzes_StudentQuizId",
                table: "StudentQuizQuestions",
                column: "StudentQuizId",
                principalTable: "StudentQuizzes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizQuestions_Questions_QuestionId",
                table: "StudentQuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzes_StudentQuizId",
                table: "StudentQuizQuestions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBbgsBexmUiphoz8rrOk10m7I6cZ1QyIbZD43wsg7o+M6AhfAdFsuxSQK5g1JeTXcg==");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizQuestions_Questions_QuestionId",
                table: "StudentQuizQuestions",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzes_StudentQuizId",
                table: "StudentQuizQuestions",
                column: "StudentQuizId",
                principalTable: "StudentQuizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
