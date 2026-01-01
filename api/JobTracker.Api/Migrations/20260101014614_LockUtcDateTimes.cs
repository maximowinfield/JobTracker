using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    public partial class LockUtcDateTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Clean invalid/quoted date strings ONLY when the column is TEXT
            migrationBuilder.Sql("""
DO $$
DECLARE users_created_type text;
DECLARE jobs_created_type text;
DECLARE jobs_updated_type text;
BEGIN
  SELECT data_type INTO users_created_type
  FROM information_schema.columns
  WHERE table_schema = 'public' AND table_name = 'Users' AND column_name = 'CreatedAtUtc';

  IF users_created_type = 'text' THEN
    UPDATE "Users"
    SET "CreatedAtUtc" = NULL
    WHERE "CreatedAtUtc" IS NOT NULL
      AND (
        btrim("CreatedAtUtc") IN ('', '""', '"', 'null', 'NULL')
        OR btrim("CreatedAtUtc", ' "') IN ('', 'null', 'NULL')
      );
  END IF;

  SELECT data_type INTO jobs_created_type
  FROM information_schema.columns
  WHERE table_schema = 'public' AND table_name = 'JobApplications' AND column_name = 'CreatedAtUtc';

  IF jobs_created_type = 'text' THEN
    UPDATE "JobApplications"
    SET "CreatedAtUtc" = NULL
    WHERE "CreatedAtUtc" IS NOT NULL
      AND (
        btrim("CreatedAtUtc") IN ('', '""', '"', 'null', 'NULL')
        OR btrim("CreatedAtUtc", ' "') IN ('', 'null', 'NULL')
      );
  END IF;

  SELECT data_type INTO jobs_updated_type
  FROM information_schema.columns
  WHERE table_schema = 'public' AND table_name = 'JobApplications' AND column_name = 'UpdatedAtUtc';

  IF jobs_updated_type = 'text' THEN
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

            // 2) Convert Users.CreatedAtUtc -> timestamptz (treat values as UTC)
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

            // 3) Convert JobApplications.CreatedAtUtc -> timestamptz
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

            // 4) Convert JobApplications.UpdatedAtUtc -> timestamptz
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
