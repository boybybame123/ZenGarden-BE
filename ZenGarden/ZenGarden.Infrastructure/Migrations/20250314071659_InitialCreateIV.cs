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
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "BaseXP",
                table: "XpConfigs",
                newName: "BaseXp");

            migrationBuilder.AlterColumn<double>(
                name: "TotalXp",
                table: "UserTree",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "UserTree",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "ChallengeRole",
                table: "UserChallenges",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "TaskDescription",
                table: "Tasks",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Tasks",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Tasks",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskNote",
                table: "Tasks",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TaskResult",
                table: "Tasks",
                type: "varchar(2083)",
                maxLength: 2083,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "WorkDuration",
                table: "Tasks",
                type: "int",
                nullable: true,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "Challenge",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "UserTree");

            migrationBuilder.DropColumn(
                name: "ChallengeRole",
                table: "UserChallenges");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskNote",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskResult",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "WorkDuration",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Challenge");

            migrationBuilder.RenameColumn(
                name: "BaseXp",
                table: "XpConfigs",
                newName: "BaseXP");

            migrationBuilder.AlterColumn<int>(
                name: "TotalXp",
                table: "UserTree",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "double",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "TaskDescription",
                table: "Tasks",
                type: "text",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Tasks",
                type: "int",
                nullable: true);
        }
    }
}
