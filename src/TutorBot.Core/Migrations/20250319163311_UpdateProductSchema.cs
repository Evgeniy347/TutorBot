using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TutorBot.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MessageHistories",
                newName: "OrderID");

            migrationBuilder.AddColumn<long>(
                name: "ChatID",
                table: "MessageHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MessageHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    CountMessages = table.Column<long>(type: "bigint", nullable: false),
                    GroupNumber = table.Column<string>(type: "text", nullable: false),
                    TimeCreate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeLastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFirstMessage = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ChatID",
                table: "Chats",
                column: "ChatID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropColumn(
                name: "ChatID",
                table: "MessageHistories");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MessageHistories");

            migrationBuilder.RenameColumn(
                name: "OrderID",
                table: "MessageHistories",
                newName: "UserId");
        }
    }
}
