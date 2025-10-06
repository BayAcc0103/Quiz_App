using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeCreatedByNullableInQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Quizzes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBWp3dGAQsbOFtlWds/uiDowTWU2C7m/P0HvQez7/yw3Rd/Ukl5RZUntlMkyefkqbg==");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEE+A+U866RFpSMiKMThr77Hir10CNId+tLAfcl2q3e4t62X04xrXw0ZkOLw1B/99w==");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Users_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
