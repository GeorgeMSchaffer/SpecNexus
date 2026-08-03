using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgDescriptionAndIdeaIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "organizations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ideas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ideas");
        }
    }
}
