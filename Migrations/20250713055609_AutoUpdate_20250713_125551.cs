using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flowganized.Migrations
{
    /// <inheritdoc />
    public partial class AutoUpdate_20250713_125551 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Hierarchies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Hierarchies");
        }
    }
}
