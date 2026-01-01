using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixAttachmentTimestamptz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Convert CreatedAtUtc from TEXT -> timestamptz safely
            //    - NULLIF handles empty strings ("") by turning them into NULL
            //    - COALESCE ensures NOT NULL by filling NULLs with NOW()
            migrationBuilder.Sql(@"
        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" DROP DEFAULT;

        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" TYPE timestamptz
        USING COALESCE(NULLIF(BTRIM(""CreatedAtUtc""), '')::timestamptz, NOW());

        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" SET NOT NULL;

        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" SET DEFAULT NOW();
    ");

            // 2) Add DeletedAtUtc (nullable)
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Attachments",
                type: "timestamptz",
                nullable: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Attachments");

            migrationBuilder.Sql(@"
        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" DROP DEFAULT;

        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" TYPE TEXT
        USING (""CreatedAtUtc""::text);

        ALTER TABLE ""Attachments""
        ALTER COLUMN ""CreatedAtUtc"" SET NOT NULL;
    ");
        }
    }
}