using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorBot.Core.Migrations
{
    /// <inheritdoc />
    public partial class addChatUserID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserID",
                table: "MessageHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "MessageHistories");
        }
    }
}
