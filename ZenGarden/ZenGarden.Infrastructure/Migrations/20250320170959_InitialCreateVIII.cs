using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZenGarden.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateVIII : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Multiplier",
                table: "XpConfigs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "XpConfigs",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<double>(
                name: "XpMultiplier",
                table: "XpConfigs",
                type: "double",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AddColumn<int>(
                name: "TreeID",
                table: "UserTree",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TaskType",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TaskType",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<double>(
                name: "XpMultiplier",
                table: "TaskType",
                type: "double",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "MinDuration",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                defaultValue: 25,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MinBreak",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxDuration",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                defaultValue: 60,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxBreak",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                defaultValue: 15,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "IsActive",
                table: "FocusMethod",
                type: "bit",
                nullable: false,
                defaultValue: 1ul,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultDuration",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                defaultValue: 25,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultBreak",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "XpMultiplier",
                table: "FocusMethod",
                type: "double",
                nullable: false,
                defaultValue: 1.0);

            migrationBuilder.CreateIndex(
                name: "IX_UserTree_TreeID",
                table: "UserTree",
                column: "TreeID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTree_Users_TreeID",
                table: "UserTree",
                column: "TreeID",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTree_Users_TreeID",
                table: "UserTree");

            migrationBuilder.DropIndex(
                name: "IX_UserTree_TreeID",
                table: "UserTree");

            migrationBuilder.DropColumn(
                name: "XpMultiplier",
                table: "XpConfigs");

            migrationBuilder.DropColumn(
                name: "TreeID",
                table: "UserTree");

            migrationBuilder.DropColumn(
                name: "XpMultiplier",
                table: "TaskType");

            migrationBuilder.DropColumn(
                name: "XpMultiplier",
                table: "FocusMethod");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "XpConfigs",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AddColumn<double>(
                name: "Multiplier",
                table: "XpConfigs",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TaskType",
                type: "datetime",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TaskType",
                type: "datetime",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "MinDuration",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 25);

            migrationBuilder.AlterColumn<int>(
                name: "MinBreak",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<int>(
                name: "MaxDuration",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 60);

            migrationBuilder.AlterColumn<int>(
                name: "MaxBreak",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 15);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "FocusMethod",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "bit",
                oldDefaultValue: 1ul);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultDuration",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 25);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultBreak",
                table: "FocusMethod",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 5);
        }
    }
}
