-- Rollback for SeedInitialData - removes all seeded rows in reverse order
-- of insertion to respect foreign key constraints.

DELETE FROM Items WHERE ContainerId IN (1, 2);
DELETE FROM PlayerAbilities WHERE PlayersId = 1 AND AbilitiesId = 1;
DELETE FROM Abilities WHERE Id = 1;
DELETE FROM Monsters WHERE Id = 1;
DELETE FROM Players WHERE Id = 1;
DELETE FROM Containers WHERE Id IN (1, 2);
