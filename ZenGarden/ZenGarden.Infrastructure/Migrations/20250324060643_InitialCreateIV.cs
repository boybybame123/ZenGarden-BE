using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateIV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_TreeOwnerA",
                table: "TradeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_TreeOwnerB",
                table: "TradeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_Tree_TreeAid",
                table: "TradeHistory");

            migrationBuilder.AddColumn<int>(
                name: "CompletedTasks",
                table: "UserChallenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TreeOwnerBUserId",
                table: "TradeHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Challenge",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Challenge",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_TreeOwnerBUserId",
                table: "TradeHistory",
                column: "TreeOwnerBUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_UserTree_TreeAid",
                table: "TradeHistory",
                column: "TreeAid",
                principalTable: "UserTree",
                principalColumn: "UserTreeID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_Users_TreeOwnerBUserId",
                table: "TradeHistory",
                column: "TreeOwnerBUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_Users_TreeOwnerBid",
                table: "TradeHistory",
                column: "TreeOwnerBid",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_UserTree_TreeAid",
                table: "TradeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_Users_TreeOwnerBUserId",
                table: "TradeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_Users_TreeOwnerBid",
                table: "TradeHistory");

            migrationBuilder.DropIndex(
                name: "IX_TradeHistory_TreeOwnerBUserId",
                table: "TradeHistory");

            migrationBuilder.DropColumn(
                name: "CompletedTasks",
                table: "UserChallenges");

            migrationBuilder.DropColumn(
                name: "TreeOwnerBUserId",
                table: "TradeHistory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Challenge",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Challenge",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_TreeOwnerA",
                table: "TradeHistory",
                column: "TreeOwnerAid",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_TreeOwnerB",
                table: "TradeHistory",
                column: "TreeOwnerBid",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_Tree_TreeAid",
                table: "TradeHistory",
                column: "TreeAid",
                principalTable: "Tree",
                principalColumn: "TreeID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
