using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizBookmarkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizBookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuizName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookmarkedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizBookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizBookmarks_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENxg073oHuMRTjHFMDLX2yrwNm6r9MRkclNplVBgoGZ0GElasJQnmE36uY8AJSxyHw==");

            migrationBuilder.CreateIndex(
                name: "IX_QuizBookmarks_QuizId",
                table: "QuizBookmarks",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizBookmarks_UserId",
                table: "QuizBookmarks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizBookmarks");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEfbnaMs0VtByxch+9ZRU9UcyQqyhhFARXXWk1av8H3zwpVG8VPBYPTSobOfq5f3aw==");
        }
    }
}
