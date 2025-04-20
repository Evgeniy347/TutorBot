using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorBot.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CountMessages",
                table: "Chats",
                newName: "MessagesCount");

            migrationBuilder.AddColumn<bool>(
                name: "EnableAdminError",
                table: "Chats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Chats",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableAdminError",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "MessagesCount",
                table: "Chats",
                newName: "CountMessages");
        }
    }
}
