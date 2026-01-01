using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    public partial class LockUtcDateTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users.CreatedAtUtc -> timestamptz
            // Handles TEXT or timestamp-ish previous types safely.
            migrationBuilder.Sql("""
ALTER TABLE "Users"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN pg_typeof("CreatedAtUtc")::text = 'text' THEN ("CreatedAtUtc")::timestamp AT TIME ZONE 'UTC'
    ELSE ("CreatedAtUtc") AT TIME ZONE 'UTC'
  END
);
""");

            // JobApplications.CreatedAtUtc -> timestamptz
            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN pg_typeof("CreatedAtUtc")::text = 'text' THEN ("CreatedAtUtc")::timestamp AT TIME ZONE 'UTC'
    ELSE ("CreatedAtUtc") AT TIME ZONE 'UTC'
  END
);
""");

            // JobApplications.UpdatedAtUtc -> timestamptz
            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "UpdatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN pg_typeof("UpdatedAtUtc")::text = 'text' THEN ("UpdatedAtUtc")::timestamp AT TIME ZONE 'UTC'
    ELSE ("UpdatedAtUtc") AT TIME ZONE 'UTC'
  END
);
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to timestamp without time zone (or text if you truly want TEXT again).
            // I'd recommend timestamp (without tz) as the "Down" target rather than TEXT.
            migrationBuilder.Sql("""
ALTER TABLE "Users"
ALTER COLUMN "CreatedAtUtc" TYPE timestamp
USING ("CreatedAtUtc" AT TIME ZONE 'UTC');
""");

            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "CreatedAtUtc" TYPE timestamp
USING ("CreatedAtUtc" AT TIME ZONE 'UTC');
""");

            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "UpdatedAtUtc" TYPE timestamp
USING ("UpdatedAtUtc" AT TIME ZONE 'UTC');
""");
        }
    }
}
