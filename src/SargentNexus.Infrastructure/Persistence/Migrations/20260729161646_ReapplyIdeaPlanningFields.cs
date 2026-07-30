using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReapplyIdeaPlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "notification_events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalState",
                table: "ideas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingApprovalExpiresAtUtc",
                table: "ideas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingApprovalPreviousStatusId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingApprovalRequestedAtUtc",
                table: "ideas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingApprovalTargetStatusId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "notification_events");

            migrationBuilder.DropColumn(
                name: "ApprovalState",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "PendingApprovalExpiresAtUtc",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "PendingApprovalPreviousStatusId",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "PendingApprovalRequestedAtUtc",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "PendingApprovalTargetStatusId",
                table: "ideas");
        }
    }
}
