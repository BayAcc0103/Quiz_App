using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizIdToRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the RoomQuizzes table as we're switching to direct relationship
            migrationBuilder.DropTable(
                name: "RoomQuizzes");

            // Add the QuizId column to Rooms table for direct relationship
            migrationBuilder.AddColumn<Guid>(
                name: "QuizId",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEKnLVzZ/kOYmlTHrVS70WXdP01Tv6EHienpRYFylDfun6JUW+vN+Ss5SXf6UQKxvlA==");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_QuizId",
                table: "Rooms",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Quizzes_QuizId",
                table: "Rooms",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Quizzes_QuizId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_QuizId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "Rooms");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEC7NOmC8ABttQxkdsSram9sdXPys4WcUMhBsx1yA5OtfkdG6xX0jZusnbferTo881Q==");

            // Recreate the RoomQuizzes table for many-to-many relationship
            migrationBuilder.CreateTable(
                name: "RoomQuizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomQuizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomQuizzes_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomQuizzes_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomQuizzes_QuizId",
                table: "RoomQuizzes",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomQuizzes_RoomId",
                table: "RoomQuizzes",
                column: "RoomId");
        }
    }
}
