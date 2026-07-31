using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardIsArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "statuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "statuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "statuses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "boards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "statuses");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "statuses");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "statuses");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "boards");
        }
    }
}
