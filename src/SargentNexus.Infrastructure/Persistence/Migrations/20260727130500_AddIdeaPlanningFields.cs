using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations;

public partial class AddIdeaPlanningFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "AssigneeUserId",
            table: "ideas",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<DateOnly>(
            name: "DueDate",
            table: "ideas",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Priority",
            table: "ideas",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Medium");

        migrationBuilder.CreateIndex(
            name: "IX_ideas_AssigneeUserId",
            table: "ideas",
            column: "AssigneeUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_ideas_users_AssigneeUserId",
            table: "ideas",
            column: "AssigneeUserId",
            principalTable: "users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ideas_users_AssigneeUserId",
            table: "ideas");

        migrationBuilder.DropIndex(
            name: "IX_ideas_AssigneeUserId",
            table: "ideas");

        migrationBuilder.DropColumn(
            name: "AssigneeUserId",
            table: "ideas");

        migrationBuilder.DropColumn(
            name: "DueDate",
            table: "ideas");

        migrationBuilder.DropColumn(
            name: "Priority",
            table: "ideas");
    }
}
