using Microsoft.EntityFrameworkCore.Migrations;

namespace Netpips.Migrations
{
    public partial class downloaditem_external_id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TvShowSubscription_Users_UserId",
                table: "TvShowSubscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TvShowSubscription",
                table: "TvShowSubscription");

            migrationBuilder.DropIndex(
                name: "IX_TvShowSubscription_UserId",
                table: "TvShowSubscription");

            migrationBuilder.RenameTable(
                name: "TvShowSubscription",
                newName: "TvShowSubscriptions");

            migrationBuilder.AlterColumn<int>(
                name: "ShowRssId",
                table: "DownloadItems",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TvMazeShowId",
                table: "DownloadItems",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TvShowSubscriptions",
                table: "TvShowSubscriptions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TvShowSubscriptions_UserId_ShowRssId",
                table: "TvShowSubscriptions",
                columns: new[] { "UserId", "ShowRssId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TvShowSubscriptions_Users_UserId",
                table: "TvShowSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TvShowSubscriptions_Users_UserId",
                table: "TvShowSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TvShowSubscriptions",
                table: "TvShowSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_TvShowSubscriptions_UserId_ShowRssId",
                table: "TvShowSubscriptions");

            migrationBuilder.DropColumn(
                name: "TvMazeShowId",
                table: "DownloadItems");

            migrationBuilder.RenameTable(
                name: "TvShowSubscriptions",
                newName: "TvShowSubscription");

            migrationBuilder.AlterColumn<string>(
                name: "ShowRssId",
                table: "DownloadItems",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TvShowSubscription",
                table: "TvShowSubscription",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TvShowSubscription_UserId",
                table: "TvShowSubscription",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TvShowSubscription_Users_UserId",
                table: "TvShowSubscription",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
