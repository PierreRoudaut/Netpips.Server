using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Netpips.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    GivenName = table.Column<string>(nullable: true),
                    FamilyName = table.Column<string>(nullable: true),
                    Picture = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DownloadItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    TotalSize = table.Column<long>(nullable: false),
                    FileUrl = table.Column<string>(nullable: true),
                    StartedAt = table.Column<DateTime>(nullable: false),
                    CompletedAt = table.Column<DateTime>(nullable: false),
                    DownloadedAt = table.Column<DateTime>(nullable: false),
                    CanceledAt = table.Column<DateTime>(nullable: false),
                    State = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Hash = table.Column<string>(nullable: true),
                    Archived = table.Column<bool>(nullable: false, defaultValue: false),
                    MovedFiles = table.Column<string>(nullable: true),
                    OwnerId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadItems_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadItems_OwnerId",
                table: "DownloadItems",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadItems");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
