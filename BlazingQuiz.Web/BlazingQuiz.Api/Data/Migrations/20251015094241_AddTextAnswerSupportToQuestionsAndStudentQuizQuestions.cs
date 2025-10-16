using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTextAnswerSupportToQuestionsAndStudentQuizQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextAnswer",
                table: "StudentQuizQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTextAnswer",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TextAnswer",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHk5YYe4OB9XGdF4LNioJ0x6BXbzI8EEV5EYAHVr2Ji9U3fQqz6DQeVQsslpwpSjJw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextAnswer",
                table: "StudentQuizQuestions");

            migrationBuilder.DropColumn(
                name: "IsTextAnswer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "TextAnswer",
                table: "Questions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEM80/SQuIqhVT4i2KfxXKN0Ov9N/UvycGjEQlozKLHLUWYkM/I/IyfW4TbRP59Vbvw==");
        }
    }
}
