using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertQuizFeedbackScoreToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a temporary column to store integer values
            migrationBuilder.AddColumn<int>(
                name: "ScoreTemp",
                table: "QuizFeedbacks",
                type: "int",
                nullable: true);

            // Update the temporary column with integer values based on text values
            migrationBuilder.Sql(@"
                UPDATE QuizFeedbacks
                SET ScoreTemp =
                    CASE
                        WHEN Score = 'very bad' THEN 1
                        WHEN Score = 'bad' THEN 2
                        WHEN Score = 'normal' THEN 3
                        WHEN Score = 'good' THEN 4
                        WHEN Score = 'very good' THEN 5
                        ELSE NULL
                    END
                WHERE Score IS NOT NULL");

            // Drop the old Score column
            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizFeedbacks");

            // Rename the temporary column to Score
            migrationBuilder.RenameColumn(
                name: "ScoreTemp",
                table: "QuizFeedbacks",
                newName: "Score");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELFB+5CkPVyYeLUcNyEZKHDKFrV8RiuahPyLegTVY73/WnfwbKAArmCYc4azGL/oPw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Create a temporary column to store string values
            migrationBuilder.AddColumn<string>(
                name: "ScoreTemp",
                table: "QuizFeedbacks",
                type: "nvarchar(max)",
                nullable: true);

            // Update the temporary column with string values based on integer values
            migrationBuilder.Sql(@"
                UPDATE QuizFeedbacks
                SET ScoreTemp =
                    CASE
                        WHEN Score = 1 THEN 'very bad'
                        WHEN Score = 2 THEN 'bad'
                        WHEN Score = 3 THEN 'normal'
                        WHEN Score = 4 THEN 'good'
                        WHEN Score = 5 THEN 'very good'
                        ELSE NULL
                    END
                WHERE Score IS NOT NULL");

            // Drop the old Score column
            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizFeedbacks");

            // Rename the temporary column to Score
            migrationBuilder.RenameColumn(
                name: "ScoreTemp",
                table: "QuizFeedbacks",
                newName: "Score");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEKueB2/Snb9ucA0U6/h7h0/GuqhyR4l6mH4tIRHN8A8FltvUdF9ti/iIff97dDj/fQ==");
        }
    }
}
