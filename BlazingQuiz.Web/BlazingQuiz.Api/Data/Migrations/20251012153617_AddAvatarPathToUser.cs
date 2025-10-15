using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarPathToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AvatarPath", "PasswordHash" },
                values: new object[] { null, "AQAAAAIAAYagAAAAEM80/SQuIqhVT4i2KfxXKN0Ov9N/UvycGjEQlozKLHLUWYkM/I/IyfW4TbRP59Vbvw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENxg073oHuMRTjHFMDLX2yrwNm6r9MRkclNplVBgoGZ0GElasJQnmE36uY8AJSxyHw==");
        }
    }
}
