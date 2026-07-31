using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationInviteCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "organizations",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "InviteCodeGeneratedAtUtc",
                table: "organizations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_organizations_InviteCode",
                table: "organizations",
                column: "InviteCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organizations_InviteCode",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "InviteCodeGeneratedAtUtc",
                table: "organizations");
        }
    }
}
