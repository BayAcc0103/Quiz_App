using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 1); // Assuming user ID 1 is an admin or default user

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEE+A+U866RFpSMiKMThr77Hir10CNId+tLAfcl2q3e4t62X04xrXw0ZkOLw1B/99w==");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_CreatedBy",
                table: "Quizzes",
                column: "CreatedBy");

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

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_CreatedBy",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Quizzes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEUDVbIXbx6kR16kF02LiEPuujHhsf7btO1H5V5dTIDggvkAt2vVMreq6dQVeVyMjA==");
        }
    }
}
