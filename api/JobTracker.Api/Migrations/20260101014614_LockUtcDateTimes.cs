using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    public partial class LockUtcDateTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean invalid/quoted date strings BEFORE converting.
            // This specifically handles values like: '', '""', '"', ' null ', '"2025-01-01T00:00:00Z"', etc.
            migrationBuilder.Sql("""
DO $$
BEGIN
  -- Users.CreatedAtUtc cleanup (only meaningful if the column is text)
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_name = 'Users' AND column_name = 'CreatedAtUtc'
  ) THEN
    UPDATE "Users"
    SET "CreatedAtUtc" = NULL
    WHERE "CreatedAtUtc" IS NOT NULL
      AND (
        btrim("CreatedAtUtc") IN ('', '""', '"', 'null', 'NULL')
        OR btrim("CreatedAtUtc", ' "') IN ('', 'null', 'NULL')
      );
  END IF;

  -- JobApplications.CreatedAtUtc cleanup
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_name = 'JobApplications' AND column_name = 'CreatedAtUtc'
  ) THEN
    UPDATE "JobApplications"
    SET "CreatedAtUtc" = NULL
    WHERE "CreatedAtUtc" IS NOT NULL
      AND (
        btrim("CreatedAtUtc") IN ('', '""', '"', 'null', 'NULL')
        OR btrim("CreatedAtUtc", ' "') IN ('', 'null', 'NULL')
      );
  END IF;

  -- JobApplications.UpdatedAtUtc cleanup
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_name = 'JobApplications' AND column_name = 'UpdatedAtUtc'
  ) THEN
    UPDATE "JobApplications"
    SET "UpdatedAtUtc" = NULL
    WHERE "UpdatedAtUtc" IS NOT NULL
      AND (
        btrim("UpdatedAtUtc") IN ('', '""', '"', 'null', 'NULL')
        OR btrim("UpdatedAtUtc", ' "') IN ('', 'null', 'NULL')
      );
  END IF;
END $$;
""");

            // Convert Users.CreatedAtUtc -> timestamptz (treat values as UTC)
            migrationBuilder.Sql("""
ALTER TABLE "Users"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN "CreatedAtUtc" IS NULL THEN NULL
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp with time zone' THEN "CreatedAtUtc"
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp without time zone' THEN ("CreatedAtUtc" AT TIME ZONE 'UTC')
    WHEN pg_typeof("CreatedAtUtc")::text = 'text' THEN (btrim("CreatedAtUtc", ' "')::timestamp AT TIME ZONE 'UTC')
    ELSE ("CreatedAtUtc"::timestamp AT TIME ZONE 'UTC')
  END
);
""");

            // Convert JobApplications.CreatedAtUtc -> timestamptz
            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN "CreatedAtUtc" IS NULL THEN NULL
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp with time zone' THEN "CreatedAtUtc"
    WHEN pg_typeof("CreatedAtUtc")::text = 'timestamp without time zone' THEN ("CreatedAtUtc" AT TIME ZONE 'UTC')
    WHEN pg_typeof("CreatedAtUtc")::text = 'text' THEN (btrim("CreatedAtUtc", ' "')::timestamp AT TIME ZONE 'UTC')
    ELSE ("CreatedAtUtc"::timestamp AT TIME ZONE 'UTC')
  END
);
""");

            // Convert JobApplications.UpdatedAtUtc -> timestamptz
            migrationBuilder.Sql("""
ALTER TABLE "JobApplications"
ALTER COLUMN "UpdatedAtUtc" TYPE timestamptz
USING (
  CASE
    WHEN "UpdatedAtUtc" IS NULL THEN NULL
    WHEN pg_typeof("UpdatedAtUtc")::text = 'timestamp with time zone' THEN "UpdatedAtUtc"
    WHEN pg_typeof("UpdatedAtUtc")::text = 'timestamp without time zone' THEN ("UpdatedAtUtc" AT TIME ZONE 'UTC')
    WHEN pg_typeof("UpdatedAtUtc")::text = 'text' THEN (btrim("UpdatedAtUtc", ' "')::timestamp AT TIME ZONE 'UTC')
    ELSE ("UpdatedAtUtc"::timestamp AT TIME ZONE 'UTC')
  END
);
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to timestamp without time zone
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
