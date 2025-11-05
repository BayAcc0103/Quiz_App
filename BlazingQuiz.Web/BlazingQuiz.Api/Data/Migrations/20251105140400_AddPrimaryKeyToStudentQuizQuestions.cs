using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryKeyToStudentQuizQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the Id column as an identity column
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "StudentQuizQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Make the composite key columns nullable temporarily to allow the primary key change
            migrationBuilder.AlterColumn<int>(
                name: "QuestionId",
                table: "StudentQuizQuestions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StudentQuizId",
                table: "StudentQuizQuestions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Set the new Id column as the primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentQuizQuestions",
                table: "StudentQuizQuestions",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentQuizQuestions",
                table: "StudentQuizQuestions");

            // Drop the Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "StudentQuizQuestions");
            
            // The down migration would need to recreate the original composite key, 
            // but this is complex and may not be safe, so it's better to keep this simple
        }
    }
}