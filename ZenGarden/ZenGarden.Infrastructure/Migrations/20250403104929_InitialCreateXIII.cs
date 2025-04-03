using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateXIII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeTask_Tasks_ChallengeID",
                table: "ChallengeTask");

            migrationBuilder.AddColumn<int>(
                name: "CloneFromTaskId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeTask_TaskID",
                table: "ChallengeTask",
                column: "TaskID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeTask_Tasks_TaskID",
                table: "ChallengeTask",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "TaskID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeTask_Tasks_TaskID",
                table: "ChallengeTask");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeTask_TaskID",
                table: "ChallengeTask");

            migrationBuilder.DropColumn(
                name: "CloneFromTaskId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeTask_Tasks_ChallengeID",
                table: "ChallengeTask",
                column: "ChallengeID",
                principalTable: "Tasks",
                principalColumn: "TaskID");
        }
    }
}
