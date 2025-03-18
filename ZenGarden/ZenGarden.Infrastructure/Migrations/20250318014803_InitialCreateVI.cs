using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateVI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_FocusMethod_FocusMethodID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserExperience_UserXpConfig_LevelId",
                table: "UserExperience");

            migrationBuilder.RenameColumn(
                name: "LevelId",
                table: "UserExperience",
                newName: "LevelID");

            migrationBuilder.RenameIndex(
                name: "IX_UserExperience_LevelId",
                table: "UserExperience",
                newName: "IX_UserExperience_LevelID");

            migrationBuilder.AlterColumn<double>(
                name: "XpAmount",
                table: "UserXpLog",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "TotalXP",
                table: "UserExperience",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserExperience",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<ulong>(
                name: "IsMaxLevel",
                table: "UserExperience",
                type: "bit",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "StreakDays",
                table: "UserExperience",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "XpAmount",
                table: "TreeXpLog",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tree",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaskResult",
                table: "Tasks",
                type: "varchar(2083)",
                maxLength: 2083,
                nullable: true,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "varchar(2083)",
                oldMaxLength: 2083,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BreakTime",
                table: "Tasks",
                type: "int",
                nullable: true,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "TotalDuration",
                table: "Tasks",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_FocusMethod_FocusMethodID",
                table: "Tasks",
                column: "FocusMethodID",
                principalTable: "FocusMethod",
                principalColumn: "FocusMethodID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserExperience_UserXpConfig_LevelID",
                table: "UserExperience",
                column: "LevelID",
                principalTable: "UserXpConfig",
                principalColumn: "LevelID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_FocusMethod_FocusMethodID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserExperience_UserXpConfig_LevelID",
                table: "UserExperience");

            migrationBuilder.DropColumn(
                name: "IsMaxLevel",
                table: "UserExperience");

            migrationBuilder.DropColumn(
                name: "StreakDays",
                table: "UserExperience");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tree");

            migrationBuilder.DropColumn(
                name: "TotalDuration",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "LevelID",
                table: "UserExperience",
                newName: "LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_UserExperience_LevelID",
                table: "UserExperience",
                newName: "IX_UserExperience_LevelId");

            migrationBuilder.AlterColumn<int>(
                name: "XpAmount",
                table: "UserXpLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "double",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<long>(
                name: "TotalXP",
                table: "UserExperience",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserExperience",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "XpAmount",
                table: "TreeXpLog",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<string>(
                name: "TaskResult",
                table: "Tasks",
                type: "varchar(2083)",
                maxLength: 2083,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "varchar(2083)",
                oldMaxLength: 2083,
                oldNullable: true,
                oldDefaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AlterColumn<int>(
                name: "BreakTime",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 5);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_FocusMethod_FocusMethodID",
                table: "Tasks",
                column: "FocusMethodID",
                principalTable: "FocusMethod",
                principalColumn: "FocusMethodID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserExperience_UserXpConfig_LevelId",
                table: "UserExperience",
                column: "LevelId",
                principalTable: "UserXpConfig",
                principalColumn: "LevelID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
