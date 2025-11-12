using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentQuizzesForRoomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentQuizForRoomId",
                table: "StudentQuizQuestions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentQuizzesForRoom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentQuizzesForRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentQuizzesForRoom_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentQuizzesForRoom_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentQuizzesForRoom_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentQuizQuestionsForRoom",
                columns: table => new
                {
                    StudentQuizForRoomId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionId = table.Column<int>(type: "int", nullable: false),
                    TextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentQuizQuestionsForRoom", x => new { x.StudentQuizForRoomId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_StudentQuizQuestionsForRoom_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentQuizQuestionsForRoom_StudentQuizzesForRoom_StudentQuizForRoomId",
                        column: x => x.StudentQuizForRoomId,
                        principalTable: "StudentQuizzesForRoom",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizQuestionsForRoom_QuestionId",
                table: "StudentQuizQuestionsForRoom",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizzesForRoom_QuizId",
                table: "StudentQuizzesForRoom",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizzesForRoom_RoomId",
                table: "StudentQuizzesForRoom",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizzesForRoom_StudentId",
                table: "StudentQuizzesForRoom",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzesForRoom_StudentQuizForRoomId",
                table: "StudentQuizQuestions",
                column: "StudentQuizForRoomId",
                principalTable: "StudentQuizzesForRoom",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizQuestions_StudentQuizzesForRoom_StudentQuizForRoomId",
                table: "StudentQuizQuestions");

            migrationBuilder.DropTable(
                name: "StudentQuizQuestionsForRoom");

            migrationBuilder.DropTable(
                name: "StudentQuizzesForRoom");

            migrationBuilder.DropIndex(
                name: "IX_StudentQuizQuestions_StudentQuizForRoomId",
                table: "StudentQuizQuestions");

            migrationBuilder.DropColumn(
                name: "StudentQuizForRoomId",
                table: "StudentQuizQuestions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENoDH5pfLa+sws7YR/V9OaqT5MOYtRXbfxzQnGG+RZ0lvojMjdXLNi6FtzAtoCcedA==");
        }
    }
}
