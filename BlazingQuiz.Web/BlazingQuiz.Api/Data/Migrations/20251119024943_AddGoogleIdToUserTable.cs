using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleIdToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GoogleId", "PasswordHash" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEC620T1XQpRZue0nv7R6YVbisvp9lzN5viI02jCE+xkOIfQ2TMeH9MQ6urAsGK8QEg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEI3PqLXsdT5f5bJ9A80vtA3dov4qad7DAJ6Sjx0HdbAIRIY4N8zbarGDBgVfiCm7Yw==");
        }
    }
}
