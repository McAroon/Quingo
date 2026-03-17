BEGIN;

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250804164858_IsTournament'
WHERE "MigrationId" = '20250804164858_istournement';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250804170629_IsTournament2'
WHERE "MigrationId" = '20250804170629_istournement2';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250804173428_Tournament3'
WHERE "MigrationId" = '20250804173428_tournament3';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250825101034_AddTimeBonus'
WHERE "MigrationId" = '20250825101034_addtimebonus';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250829142559_LobbyType'
WHERE "MigrationId" = '20250829142559_lobbytype';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250829153228_RemoveLobbyType'
WHERE "MigrationId" = '20250829153228_removelobbytype';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250905135349_AddPosition'
WHERE "MigrationId" = '20250905135349_addposition';

UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20250920163856_IndexNew'
WHERE "MigrationId" = '20250920163856_indexnew';

COMMIT;
