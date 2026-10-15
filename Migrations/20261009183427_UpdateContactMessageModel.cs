using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareMeal.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactMessageModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "ContactMessages",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ContactMessages",
                newName: "SubmittedAt");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "ContactMessages",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ContactMessages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_UserId",
                table: "ContactMessages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_AspNetUsers_UserId",
                table: "ContactMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_AspNetUsers_UserId",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_UserId",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ContactMessages");

            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                table: "ContactMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ContactMessages",
                newName: "FullName");
        }
    }
}
