using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationLogoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LogoHeightPx",
                table: "organizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoThumbnailUrl",
                table: "organizations",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "organizations",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoHeightPx",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "LogoThumbnailUrl",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "organizations");
        }
    }
}
