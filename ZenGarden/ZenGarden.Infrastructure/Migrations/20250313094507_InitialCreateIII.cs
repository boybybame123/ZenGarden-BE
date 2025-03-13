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
            migrationBuilder.DropColumn(
                name: "CurrentLevel",
                table: "UserExperience");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentLevel",
                table: "UserExperience",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
