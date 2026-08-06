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
            migrationBuilder.Sql("IF COL_LENGTH('ideas', 'DueDate') IS NULL ALTER TABLE [ideas] ADD [DueDate] date NULL;");
            migrationBuilder.Sql("IF COL_LENGTH('ideas', 'Priority') IS NULL ALTER TABLE [ideas] ADD [Priority] nvarchar(20) NOT NULL CONSTRAINT [DF_ideas_Priority] DEFAULT ('Medium');");

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessImpactId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "ideas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaTypeId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.Sql(
                """
                INSERT INTO [idea_types] ([Id], [OrganizationId], [Name], [SortOrder], [IsDeleted])
                SELECT NEWID(), [organization].[Id], [defaults].[Name], [defaults].[SortOrder], 0
                FROM [organizations] AS [organization]
                CROSS JOIN (VALUES
                    (N'Continuous Improvement', 0),
                    (N'Process Revision', 1)
                ) AS [defaults] ([Name], [SortOrder]);

                INSERT INTO [business_impacts] ([Id], [OrganizationId], [Name], [Color], [SortOrder], [IsDeleted])
                SELECT NEWID(), [organization].[Id], [defaults].[Name], [defaults].[Color], [defaults].[SortOrder], 0
                FROM [organizations] AS [organization]
                CROSS JOIN (VALUES
                    (N'Low', N'#16A34A', 0),
                    (N'Medium', N'#2563EB', 1),
                    (N'High', N'#D97706', 2),
                    (N'Critical', N'#DC2626', 3)
                ) AS [defaults] ([Name], [Color], [SortOrder]);

                UPDATE [idea]
                SET [IdeaTypeId] = [idea_type].[Id],
                    [BusinessImpactId] = [business_impact].[Id]
                FROM [ideas] AS [idea]
                CROSS APPLY (
                    SELECT TOP (1) [item].[Id]
                    FROM [idea_types] AS [item]
                    WHERE [item].[OrganizationId] = [idea].[OrganizationId]
                    ORDER BY [item].[SortOrder], [item].[Id]
                ) AS [idea_type]
                CROSS APPLY (
                    SELECT TOP (1) [item].[Id]
                    FROM [business_impacts] AS [item]
                    WHERE [item].[OrganizationId] = [idea].[OrganizationId]
                    ORDER BY [item].[SortOrder], [item].[Id]
                ) AS [business_impact];

                IF COL_LENGTH('ideas', 'AssigneeUserId') IS NOT NULL
                BEGIN
                    EXEC(N'
                        INSERT INTO [idea_assignees] ([IdeaId], [UserId])
                        SELECT [Id], [AssigneeUserId]
                        FROM [ideas]
                        WHERE [AssigneeUserId] IS NOT NULL;
                    ');
                END
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "IdeaTypeId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BusinessImpactId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.Sql(
                "DECLARE @constraintName sysname; SELECT @constraintName = fk.name FROM sys.foreign_keys fk INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id INNER JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id WHERE fk.parent_object_id = OBJECT_ID(N'[ideas]') AND c.name = N'AssigneeUserId'; IF @constraintName IS NOT NULL BEGIN EXEC(N'ALTER TABLE [ideas] DROP CONSTRAINT [' + @constraintName + ']'); END");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_ideas_AssigneeUserId] ON [ideas];");
            migrationBuilder.Sql("IF COL_LENGTH('ideas', 'AssigneeUserId') IS NOT NULL EXEC(N'ALTER TABLE [ideas] DROP COLUMN [AssigneeUserId]');");

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

            migrationBuilder.DropIndex(
                name: "IX_ideas_BusinessImpactId",
                table: "ideas");

            migrationBuilder.DropIndex(
                name: "IX_ideas_IdeaTypeId",
                table: "ideas");

            migrationBuilder.AddColumn<Guid>(
                name: "AssigneeUserId",
                table: "ideas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [idea]
                SET [AssigneeUserId] = [assignee].[UserId]
                FROM [ideas] AS [idea]
                CROSS APPLY (
                    SELECT TOP (1) [item].[UserId]
                    FROM [idea_assignees] AS [item]
                    WHERE [item].[IdeaId] = [idea].[Id]
                    ORDER BY [item].[UserId]
                ) AS [assignee];
                """);

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

            migrationBuilder.DropTable(
                name: "business_impacts");

            migrationBuilder.DropTable(
                name: "idea_assignees");

            migrationBuilder.DropTable(
                name: "idea_types");

            migrationBuilder.DropColumn(
                name: "BusinessImpactId",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ideas");

            migrationBuilder.DropColumn(
                name: "IdeaTypeId",
                table: "ideas");
        }
    }
}






