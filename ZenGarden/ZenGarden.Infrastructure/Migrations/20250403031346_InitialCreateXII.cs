using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateXII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskType_TaskTypeID",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskType_TaskTypeID",
                table: "Tasks",
                column: "TaskTypeID",
                principalTable: "TaskType",
                principalColumn: "TaskTypeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskType_TaskTypeID",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskType_TaskTypeID",
                table: "Tasks",
                column: "TaskTypeID",
                principalTable: "TaskType",
                principalColumn: "TaskTypeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
