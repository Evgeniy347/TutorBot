using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorBot.Core.Migrations
{
    /// <inheritdoc />
    public partial class migration_18_04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageHistories_ChatID_OrderID",
                table: "MessageHistories");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ServiceHistories",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "MessageHistories",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionID",
                table: "MessageHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeLastUpdate",
                table: "Chats",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeCreate",
                table: "Chats",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "LastActionKey",
                table: "Chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionID",
                table: "Chats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistories_ChatID_OrderID_SessionID",
                table: "MessageHistories",
                columns: new[] { "ChatID", "OrderID", "SessionID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageHistories_ChatID_OrderID_SessionID",
                table: "MessageHistories");

            migrationBuilder.DropColumn(
                name: "SessionID",
                table: "MessageHistories");

            migrationBuilder.DropColumn(
                name: "LastActionKey",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "SessionID",
                table: "Chats");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ServiceHistories",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "MessageHistories",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeLastUpdate",
                table: "Chats",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeCreate",
                table: "Chats",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistories_ChatID_OrderID",
                table: "MessageHistories",
                columns: new[] { "ChatID", "OrderID" });
        }
    }
}
