using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateVII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "challengetask_ibfk_1",
                table: "ChallengeTask");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Challenge",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Effect",
                table: "ItemDetail",
                type: "text",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddForeignKey(
                name: "challengetask_ibfk_1",
                table: "ChallengeTask",
                column: "ChallengeID",
                principalTable: "Challenge",
                principalColumn: "ChallengeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "challengetask_ibfk_1",
                table: "ChallengeTask");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Challenge",
                newName: "status");

            migrationBuilder.AlterColumn<string>(
                name: "Effect",
                table: "ItemDetail",
                type: "json",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddForeignKey(
                name: "challengetask_ibfk_1",
                table: "ChallengeTask",
                column: "ChallengeID",
                principalTable: "Challenge",
                principalColumn: "ChallengeID");
        }
    }
}
