using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    public partial class LockUtcDateTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
ALTER TABLE "Users"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN pg_typeof("CreatedAtUtc")::text = 'text'
      THEN ("CreatedAtUtc")::timestamp AT TIME ZONE 'UTC'
    ELSE ("CreatedAtUtc") AT TIME ZONE 'UTC'
  END
);
""");

            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN pg_typeof("CreatedAtUtc")::text = 'text'
      THEN ("CreatedAtUtc")::timestamp AT TIME ZONE 'UTC'
    ELSE ("CreatedAtUtc") AT TIME ZONE 'UTC'
  END
);
""");

            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "UpdatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN pg_typeof("UpdatedAtUtc")::text = 'text'
      THEN ("UpdatedAtUtc")::timestamp AT TIME ZONE 'UTC'
    ELSE ("UpdatedAtUtc") AT TIME ZONE 'UTC'
  END
);
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
