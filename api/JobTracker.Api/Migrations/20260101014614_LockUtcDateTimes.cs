using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    public partial class LockUtcDateTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Users.CreatedAtUtc
            migrationBuilder.Sql("""
ALTER TABLE "Users"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN "CreatedAtUtc" IS NULL THEN NULL
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp with time zone' THEN "CreatedAtUtc"
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp without time zone' THEN ("CreatedAtUtc" AT TIME ZONE 'UTC')
    WHEN pg_typeof("CreatedAtUtc")::text = 'text' THEN (NULLIF("CreatedAtUtc",'')::timestamp AT TIME ZONE 'UTC')
    ELSE ("CreatedAtUtc"::timestamp AT TIME ZONE 'UTC')
  END
);
""");

            // JobApplications.CreatedAtUtc
            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN "CreatedAtUtc" IS NULL THEN NULL
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp with time zone' THEN "CreatedAtUtc"
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp without time zone' THEN ("CreatedAtUtc" AT TIME ZONE 'UTC')
    WHEN pg_typeof("CreatedAtUtc")::text = 'text' THEN (NULLIF("CreatedAtUtc",'')::timestamp AT TIME ZONE 'UTC')
    ELSE ("CreatedAtUtc"::timestamp AT TIME ZONE 'UTC')
  END
);
""");

            // JobApplications.UpdatedAtUtc
            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "UpdatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN "UpdatedAtUtc" IS NULL THEN NULL
    WHEN pg_typeof("UpdatedAtUtc")::text = 'timestamp with time zone' THEN "UpdatedAtUtc"
    WHEN pg_typeof("UpdatedAtUtc")::text = 'timestamp without time zone' THEN ("UpdatedAtUtc" AT TIME ZONE 'UTC')
    WHEN pg_typeof("UpdatedAtUtc")::text = 'text' THEN (NULLIF("UpdatedAtUtc",'')::timestamp AT TIME ZONE 'UTC')
    ELSE ("UpdatedAtUtc"::timestamp AT TIME ZONE 'UTC')
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
