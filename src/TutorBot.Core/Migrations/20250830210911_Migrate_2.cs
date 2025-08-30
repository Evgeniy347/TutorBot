using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TutorBot.Core.Migrations
{
    /// <inheritdoc />
    public partial class Migrate_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageHistories_ChatID_OrderID_SessionID",
                table: "MessageHistories");

            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "MessageHistories");

            migrationBuilder.RenameColumn(
                name: "TimeLastUpdate",
                table: "Chats",
                newName: "TimeModified");

            migrationBuilder.RenameColumn(
                name: "MessagesCount",
                table: "Chats",
                newName: "Version");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Chats",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ChatsVersions",
                columns: table => new
                {
                    UniqueId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    ChatID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    GroupNumber = table.Column<string>(type: "text", nullable: false),
                    TimeCreate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeModified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsFirstMessage = table.Column<bool>(type: "boolean", nullable: false),
                    LastActionKey = table.Column<string>(type: "text", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    EnableAdminError = table.Column<bool>(type: "boolean", nullable: false),
                    SessionID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatsVersions", x => x.UniqueId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistories_ChatID_SessionID",
                table: "MessageHistories",
                columns: new[] { "ChatID", "SessionID" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatsVersions_ChatID",
                table: "ChatsVersions",
                column: "ChatID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatsVersions");

            migrationBuilder.DropIndex(
                name: "IX_MessageHistories_ChatID_SessionID",
                table: "MessageHistories");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Chats",
                newName: "MessagesCount");

            migrationBuilder.RenameColumn(
                name: "TimeModified",
                table: "Chats",
                newName: "TimeLastUpdate");

            migrationBuilder.AddColumn<long>(
                name: "OrderID",
                table: "MessageHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistories_ChatID_OrderID_SessionID",
                table: "MessageHistories",
                columns: new[] { "ChatID", "OrderID", "SessionID" });
        }
    }
}
