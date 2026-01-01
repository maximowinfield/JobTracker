using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Api.Migrations
{
    public partial class LockUtcDateTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
DECLARE t text;
BEGIN
  -- -------------------------
  -- Users.CreatedAtUtc
  -- -------------------------
  SELECT data_type INTO t
  FROM information_schema.columns
  WHERE table_schema='public' AND table_name='Users' AND column_name='CreatedAtUtc';

  IF t = 'text' THEN
    -- sanitize poison strings
    UPDATE "Users"
    SET "CreatedAtUtc" = NULL
    WHERE "CreatedAtUtc" IS NOT NULL
      AND (btrim("CreatedAtUtc", ' "') IN ('', 'null', 'NULL'));

    ALTER TABLE "Users"
    ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
    USING (btrim("CreatedAtUtc", ' "')::timestamp AT TIME ZONE 'UTC');

  ELSIF t = 'timestamp without time zone' THEN
    ALTER TABLE "Users"
    ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
    USING ("CreatedAtUtc" AT TIME ZONE 'UTC');

  ELSIF t = 'timestamp with time zone' THEN
    -- already correct; do nothing
    NULL;
  END IF;

  -- -------------------------
  -- JobApplications.CreatedAtUtc
  -- -------------------------
  SELECT data_type INTO t
  FROM information_schema.columns
  WHERE table_schema='public' AND table_name='JobApplications' AND column_name='CreatedAtUtc';

  IF t = 'text' THEN
    UPDATE "JobApplications"
    SET "CreatedAtUtc" = NULL
    WHERE "CreatedAtUtc" IS NOT NULL
      AND (btrim("CreatedAtUtc", ' "') IN ('', 'null', 'NULL'));

    ALTER TABLE "JobApplications"
    ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
    USING (btrim("CreatedAtUtc", ' "')::timestamp AT TIME ZONE 'UTC');

  ELSIF t = 'timestamp without time zone' THEN
    ALTER TABLE "JobApplications"
    ALTER COLUMN "CreatedAtUtc" TYPE timestamptz
    USING ("CreatedAtUtc" AT TIME ZONE 'UTC');

  ELSIF t = 'timestamp with time zone' THEN
    NULL;
  END IF;

  -- -------------------------
  -- JobApplications.UpdatedAtUtc
  -- -------------------------
  SELECT data_type INTO t
  FROM information_schema.columns
  WHERE table_schema='public' AND table_name='JobApplications' AND column_name='UpdatedAtUtc';

  IF t = 'text' THEN
    UPDATE "JobApplications"
    SET "UpdatedAtUtc" = NULL
    WHERE "UpdatedAtUtc" IS NOT NULL
      AND (btrim("UpdatedAtUtc", ' "') IN ('', 'null', 'NULL'));

    ALTER TABLE "JobApplications"
    ALTER COLUMN "UpdatedAtUtc" TYPE timestamptz
    USING (btrim("UpdatedAtUtc", ' "')::timestamp AT TIME ZONE 'UTC');

  ELSIF t = 'timestamp without time zone' THEN
    ALTER TABLE "JobApplications"
    ALTER COLUMN "UpdatedAtUtc" TYPE timestamptz
    USING ("UpdatedAtUtc" AT TIME ZONE 'UTC');

  ELSIF t = 'timestamp with time zone' THEN
    NULL;
  END IF;

END $$;
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
DECLARE t text;
BEGIN
  SELECT data_type INTO t
  FROM information_schema.columns
  WHERE table_schema='public' AND table_name='Users' AND column_name='CreatedAtUtc';

  IF t = 'timestamp with time zone' THEN
    ALTER TABLE "Users"
    ALTER COLUMN "CreatedAtUtc" TYPE timestamp
    USING ("CreatedAtUtc" AT TIME ZONE 'UTC');
  END IF;

  SELECT data_type INTO t
  FROM information_schema.columns
  WHERE table_schema='public' AND table_name='JobApplications' AND column_name='CreatedAtUtc';

  IF t = 'timestamp with time zone' THEN
    ALTER TABLE "JobApplications"
    ALTER COLUMN "CreatedAtUtc" TYPE timestamp
    USING ("CreatedAtUtc" AT TIME ZONE 'UTC');
  END IF;

  SELECT data_type INTO t
  FROM information_schema.columns
  WHERE table_schema='public' AND table_name='JobApplications' AND column_name='UpdatedAtUtc';

  IF t = 'timestamp with time zone' THEN
    ALTER TABLE "JobApplications"
    ALTER COLUMN "UpdatedAtUtc" TYPE timestamp
    USING ("UpdatedAtUtc" AT TIME ZONE 'UTC');
  END IF;
END $$;
""");
        }
    }
}
