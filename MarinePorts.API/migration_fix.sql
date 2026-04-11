START TRANSACTION;

ALTER TABLE "Users" ADD "FirstName" text NOT NULL DEFAULT '';

ALTER TABLE "Users" ADD "LastName" text NOT NULL DEFAULT '';

UPDATE "Users"
SET "FirstName" = SPLIT_PART("FullName", ' ', 1),
    "LastName"  = CASE
        WHEN POSITION(' ' IN "FullName") > 0
        THEN SUBSTRING("FullName" FROM POSITION(' ' IN "FullName") + 1)
        ELSE ''
    END

ALTER TABLE "Users" DROP COLUMN "FullName";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260409195015_SplitFullNameToFirstLast', '8.0.0');

COMMIT;

