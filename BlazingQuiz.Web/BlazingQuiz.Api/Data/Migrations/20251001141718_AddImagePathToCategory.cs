using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Categories",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEUDVbIXbx6kR16kF02LiEPuujHhsf7btO1H5V5dTIDggvkAt2vVMreq6dQVeVyMjA==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENS58w0qOO5ZitC8UhbawVcDn4nXM43K8tge+SHSvmkYdn5OSuZFb0J3LZZWWkDz3Q==");
        }
    }
}
