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
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.organizations', 'InviteCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[organizations]
                    ADD [InviteCode] nvarchar(16) NOT NULL CONSTRAINT [DF_organizations_InviteCode] DEFAULT N'';
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.organizations', 'InviteCodeGeneratedAtUtc') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[organizations]
                    ADD [InviteCodeGeneratedAtUtc] datetime2 NOT NULL
                        CONSTRAINT [DF_organizations_InviteCodeGeneratedAtUtc] DEFAULT '0001-01-01T00:00:00.0000000';
                END
                """);

            migrationBuilder.Sql(
                """
                WITH duplicate_codes AS (
                    SELECT [InviteCode]
                    FROM [dbo].[organizations]
                    WHERE [InviteCode] IS NOT NULL AND LTRIM(RTRIM([InviteCode])) <> N''
                    GROUP BY [InviteCode]
                    HAVING COUNT(*) > 1
                ),
                rows_to_fix AS (
                    SELECT
                        [id],
                        ROW_NUMBER() OVER (ORDER BY [id]) AS [row_number]
                    FROM [dbo].[organizations]
                    WHERE [InviteCode] IS NULL
                       OR LTRIM(RTRIM([InviteCode])) = N''
                       OR [InviteCode] IN (SELECT [InviteCode] FROM duplicate_codes)
                )
                UPDATE organization
                SET
                    [InviteCode] = CONCAT(N'ORG', RIGHT(N'00000' + CAST(rows_to_fix.[row_number] AS nvarchar(5)), 5)),
                    [InviteCodeGeneratedAtUtc] = CASE
                        WHEN organization.[InviteCodeGeneratedAtUtc] = '0001-01-01T00:00:00.0000000'
                            THEN SYSUTCDATETIME()
                        ELSE organization.[InviteCodeGeneratedAtUtc]
                    END
                FROM [dbo].[organizations] AS organization
                INNER JOIN rows_to_fix ON rows_to_fix.[id] = organization.[id];
                """);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[organizations]
                SET [InviteCodeGeneratedAtUtc] = SYSUTCDATETIME()
                WHERE [InviteCodeGeneratedAtUtc] = '0001-01-01T00:00:00.0000000';
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_organizations_InviteCode'
                      AND [object_id] = OBJECT_ID(N'[dbo].[organizations]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_organizations_InviteCode]
                        ON [dbo].[organizations] ([InviteCode]);
                END
                """);
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
