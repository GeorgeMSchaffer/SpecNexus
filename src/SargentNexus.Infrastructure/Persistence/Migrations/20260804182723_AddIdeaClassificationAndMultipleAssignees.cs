using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SargentNexus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdeaClassificationAndMultipleAssignees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @constraintName sysname; SELECT @constraintName = fk.name FROM sys.foreign_keys fk INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id WHERE fk.parent_object_id = OBJECT_ID(N'[ideas]') AND c.name = N'AssigneeUserId'; IF @constraintName IS NOT NULL BEGIN EXEC(N'ALTER TABLE [ideas] DROP CONSTRAINT [' + @constraintName + ']'); END");

            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_ideas_AssigneeUserId] ON [ideas];");

            migrationBuilder.Sql("IF COL_LENGTH('ideas', 'DeletedByUserId') IS NULL BEGIN IF COL_LENGTH('ideas', 'AssigneeUserId') IS NOT NULL BEGIN EXEC sp_rename N'[ideas].[AssigneeUserId]', N'DeletedByUserId', N'COLUMN'; END ELSE BEGIN ALTER TABLE [ideas] ADD [DeletedByUserId] uniqueidentifier NULL; END END");
            migrationBuilder.Sql("IF COL_LENGTH('ideas', 'DueDate') IS NULL ALTER TABLE [ideas] ADD [DueDate] date NULL;");
            migrationBuilder.Sql("IF COL_LENGTH('ideas', 'Priority') IS NULL ALTER TABLE [ideas] ADD [Priority] nvarchar(20) NOT NULL CONSTRAINT [DF_ideas_Priority] DEFAULT ('Medium');");
            migrationBuilder.AddColumn<Guid>(
                name: "BusinessImpactId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "ideas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaTypeId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "business_impacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_impacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_business_impacts_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "idea_assignees",
                columns: table => new
                {
                    IdeaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idea_assignees", x => new { x.IdeaId, x.UserId });
                    table.ForeignKey(
                        name: "FK_idea_assignees_ideas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "ideas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_idea_assignees_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "idea_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idea_types", x => x.Id);
                    table.ForeignKey(
                        name: "FK_idea_types_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ideas_BusinessImpactId",
                table: "ideas",
                column: "BusinessImpactId");

            migrationBuilder.CreateIndex(
                name: "IX_ideas_IdeaTypeId",
                table: "ideas",
                column: "IdeaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_business_impacts_OrganizationId_Name",
                table: "business_impacts",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_business_impacts_OrganizationId_SortOrder",
                table: "business_impacts",
                columns: new[] { "OrganizationId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_idea_assignees_UserId",
                table: "idea_assignees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_idea_types_OrganizationId_Name",
                table: "idea_types",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_idea_types_OrganizationId_SortOrder",
                table: "idea_types",
                columns: new[] { "OrganizationId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_ideas_business_impacts_BusinessImpactId",
                table: "ideas",
                column: "BusinessImpactId",
                principalTable: "business_impacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ideas_idea_types_IdeaTypeId",
                table: "ideas",
                column: "IdeaTypeId",
                principalTable: "idea_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ideas_business_impacts_BusinessImpactId",
                table: "ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_ideas_idea_types_IdeaTypeId",
                table: "ideas");

            migrationBuilder.DropTable(
                name: "business_impacts");

            migrationBuilder.DropTable(
                name: "idea_assignees");

            migrationBuilder.DropTable(
                name: "idea_types");

            migrationBuilder.DropIndex(
                name: "IX_ideas_BusinessImpactId",
                table: "ideas");

            migrationBuilder.DropIndex(
                name: "IX_ideas_IdeaTypeId",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "BusinessImpactId",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "IdeaTypeId",
                table: "ideas");

            migrationBuilder.RenameColumn(
                name: "DeletedByUserId",
                table: "ideas",
                newName: "AssigneeUserId");

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
    }
}






