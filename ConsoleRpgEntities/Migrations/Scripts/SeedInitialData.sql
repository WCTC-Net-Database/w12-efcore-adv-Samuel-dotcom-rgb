-- ================================================================
-- Week 12 Seed Data
-- ================================================================
-- Seeds one starter Player with an Inventory (holding items) and an Equipment
-- slot (empty, to be filled by the student during class). Also seeds a single
-- Goblin monster as a combat target, and one ability for the player.
--
-- All inserts are deterministic: Id=1 for Player, Inventory container,
-- Equipment container, Goblin, and the ability. The Items table gets a
-- variety of Weapons, Armor, Consumables, and one KeyItem, all dropped
-- into the Inventory so students have something to LINQ over.
-- ================================================================

-- ----------------------------------------------------------------
-- Containers (one Inventory + one Equipment, both owned by Player 1)
-- ----------------------------------------------------------------
SET IDENTITY_INSERT Containers ON;
INSERT INTO Containers (Id, ContainerType, MaxWeight)
VALUES
    (1, 'Inventory', 100),
    (2, 'Equipment', NULL);
SET IDENTITY_INSERT Containers OFF;

-- ----------------------------------------------------------------
-- Player (single starting character linked to the containers above)
-- ----------------------------------------------------------------
SET IDENTITY_INSERT Players ON;
INSERT INTO Players (Id, Name, Level, Experience, Health, InventoryId, EquipmentId)
VALUES
    (1, 'Elara the Bold', 3, 150, 100, 1, 2);
SET IDENTITY_INSERT Players OFF;

-- ----------------------------------------------------------------
-- Monster (goblin target for combat demo)
-- ----------------------------------------------------------------
SET IDENTITY_INSERT Monsters ON;
INSERT INTO Monsters (Id, Name, Health, AggressionLevel, MonsterType, Sneakiness)
VALUES
    (1, 'Grubnak the Greedy', 30, 8, 'Goblin', 6);
SET IDENTITY_INSERT Monsters OFF;

-- ----------------------------------------------------------------
-- Ability (one player ability, from Week 10 TPH pattern)
-- ----------------------------------------------------------------
SET IDENTITY_INSERT Abilities ON;
INSERT INTO Abilities (Id, Name, Description, AbilityType, Damage, Distance)
VALUES
    (1, 'Power Shove', 'A forceful shove that knocks the target back.', 'ShoveAbility', 8, 5);
SET IDENTITY_INSERT Abilities OFF;

INSERT INTO PlayerAbilities (PlayersId, AbilitiesId)
VALUES
    (1, 1);

-- ----------------------------------------------------------------
-- Items (dropped into Elara's Inventory - ContainerId = 1)
-- ----------------------------------------------------------------
-- Mix of weapons, armor, consumables, and a key item so students can
-- practice LINQ queries: Where, GroupBy, OrderBy, OfType, Sum, etc.
-- Notice the discriminator column ItemType - this is TPH in action.
-- ----------------------------------------------------------------

-- Weapons
INSERT INTO Items (Name, Description, ItemType, Weight, Value, ContainerId, Attack, Category)
VALUES
    ('Iron Shortsword',  'A reliable blade forged from plain iron.',      'Weapon',  4.00, 45,  1, 6,  'Melee'),
    ('Ashwood Bow',      'A hunter''s bow made of supple ashwood.',       'Weapon',  2.50, 80,  1, 8,  'Ranged'),
    ('Gnarled Staff',    'A twisted staff humming with faint magic.',     'Weapon',  3.00, 120, 1, 5,  'Magic'),
    ('Rusty Dagger',     'Better than nothing. Barely.',                  'Weapon',  1.00, 5,   1, 2,  'Melee');

-- Armor
INSERT INTO Items (Name, Description, ItemType, Weight, Value, ContainerId, Defense, Slot)
VALUES
    ('Leather Jerkin',   'Hardened leather vest, lightly worn.',          'Armor',   5.00, 35,  1, 4, 'Body'),
    ('Iron Helm',        'A dented but sturdy iron helmet.',              'Armor',   3.50, 25,  1, 3, 'Head'),
    ('Traveler''s Boots','Well-worn boots with thick soles.',             'Armor',   2.00, 15,  1, 1, 'Feet'),
    ('Ring of Warding',  'A silver ring inscribed with protective runes.','Armor',   0.10, 200, 1, 2, 'Ring');

-- Consumables
INSERT INTO Items (Name, Description, ItemType, Weight, Value, ContainerId, EffectType, EffectAmount, Uses)
VALUES
    ('Lesser Healing Potion', 'A small vial of red liquid.',              'Consumable', 0.50, 20, 1, 'Heal', 15, 1),
    ('Greater Healing Potion','A warm bottle that glows gently.',         'Consumable', 0.50, 60, 1, 'Heal', 40, 1),
    ('Mana Draught',          'Tastes of crushed herbs and starlight.',   'Consumable', 0.50, 45, 1, 'Mana', 25, 1),
    ('Stamina Tonic',         'A gritty brown mixture.',                  'Consumable', 0.50, 30, 1, 'Buff', 10, 3);

-- Key Item
INSERT INTO Items (Name, Description, ItemType, Weight, Value, ContainerId, KeyId)
VALUES
    ('Worn Lockpick',    'A thin metal tool, perfect for picky locks.',   'KeyItem', 0.10, 0, 1, NULL);
