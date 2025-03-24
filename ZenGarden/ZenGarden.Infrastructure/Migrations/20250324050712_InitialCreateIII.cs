using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateIII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTree_Users_TreeID",
                table: "UserTree");

            migrationBuilder.RenameColumn(
                name: "TreeID",
                table: "UserTree",
                newName: "OwnerID");

            migrationBuilder.RenameIndex(
                name: "IX_UserTree_TreeID",
                table: "UserTree",
                newName: "IX_UserTree_OwnerID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTree_Users_OwnerID",
                table: "UserTree",
                column: "OwnerID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTree_Users_OwnerID",
                table: "UserTree");

            migrationBuilder.RenameColumn(
                name: "OwnerID",
                table: "UserTree",
                newName: "TreeID");

            migrationBuilder.RenameIndex(
                name: "IX_UserTree_OwnerID",
                table: "UserTree",
                newName: "IX_UserTree_TreeID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTree_Users_TreeID",
                table: "UserTree",
                column: "TreeID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
