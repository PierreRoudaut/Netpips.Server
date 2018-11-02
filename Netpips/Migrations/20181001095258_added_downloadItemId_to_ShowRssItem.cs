using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Netpips.Migrations
{
    public partial class added_downloadItemId_to_ShowRssItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowRssId",
                table: "DownloadItems");

            migrationBuilder.DropColumn(
                name: "TvMazeShowId",
                table: "DownloadItems");

            migrationBuilder.CreateTable(
                name: "ShowRssItems",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Link = table.Column<string>(nullable: true),
                    PubDate = table.Column<DateTime>(nullable: false),
                    ShowRssId = table.Column<int>(nullable: false),
                    TvMazeShowId = table.Column<int>(nullable: false),
                    TvShowName = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    DownloadItemId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowRssItems", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_ShowRssItems_DownloadItems_DownloadItemId",
                        column: x => x.DownloadItemId,
                        principalTable: "DownloadItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShowRssItems_DownloadItemId",
                table: "ShowRssItems",
                column: "DownloadItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowRssItems");

            migrationBuilder.AddColumn<int>(
                name: "ShowRssId",
                table: "DownloadItems",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TvMazeShowId",
                table: "DownloadItems",
                nullable: true);
        }
    }
}
