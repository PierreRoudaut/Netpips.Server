using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Netpips.Migrations
{
    public partial class tvshow_subscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                nullable: false,
                defaultValue: "User",
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<bool>(
                name: "ManualDownloadEmailNotificationEnabled",
                table: "Users",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "TvShowSubscriptionEmailNotificationEnabled",
                table: "Users",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ShowRssId",
                table: "DownloadItems",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TvShowSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ShowTitle = table.Column<string>(nullable: true),
                    ShowRssId = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvShowSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TvShowSubscription_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TvShowSubscription_UserId",
                table: "TvShowSubscription",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TvShowSubscription");

            migrationBuilder.DropColumn(
                name: "ManualDownloadEmailNotificationEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TvShowSubscriptionEmailNotificationEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShowRssId",
                table: "DownloadItems");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                nullable: false,
                oldClrType: typeof(string),
                oldDefaultValue: "User");
        }
    }
}
