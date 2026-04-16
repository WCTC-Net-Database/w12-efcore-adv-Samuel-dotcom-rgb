# Week 12: Inventory & Equipment with Advanced LINQ

> **Template Purpose:** This template represents a working solution through Week 11. Use YOUR repo if you're caught up. Use this as a fresh start if needed.

---

## Overview

This week you'll build a real **Items & Containers** system using two new TPH hierarchies:

1. **`Item`** — an abstract base with four concrete subtypes: **Weapon**, **Armor**, **Consumable**, and **KeyItem**.
2. **`Container`** — an abstract base with two subtypes for now: **Inventory** (backpack) and **Equipment** (equipped slots).

Every item in the game lives in exactly one container at a time. When you pick something up, equip it, drop it, or use it, all you're really doing is changing one foreign key: `Item.ContainerId`. That's the whole magic.

With this foundation in place, you'll practice **advanced LINQ** — `Where`, `GroupBy`, `OrderBy`, `Select`, `Sum`, `OfType<T>` — to search, filter, group, and sort items in your player's inventory.

> **Looking ahead:** The Container pattern you build this week is the foundation for **Week 13** (chests and monster loot drops) and **Week 14** (items on the ground in rooms). Every "place a thing can live" in the game world will be a Container subclass.

## Learning Objectives

By completing this assignment, you will:
- [ ] Model a polymorphic `Item` hierarchy using TPH (Table-Per-Hierarchy)
- [ ] Model a polymorphic `Container` hierarchy using TPH
- [ ] Understand the difference between **item instances** and **item types** (one-to-many, NOT many-to-many)
- [ ] Use advanced LINQ (`Where`, `GroupBy`, `OrderBy`, `OfType<T>`, `Sum`) on in-memory collections
- [ ] Implement inventory operations: pick up, equip, unequip, use, drop
- [ ] Move items between containers by updating a single foreign key
- [ ] Apply a seed data migration that runs a `.sql` script

## Prerequisites

- [ ] Completed Week 11 assignment (or using this template)
- [ ] Understanding of TPH from Week 10
- [ ] Working LINQ basics (`Where`, `FirstOrDefault`, `Sum`)
- [ ] Working EF Core migrations from Week 9

---

## What's New This Week

| Concept | Description |
|---------|-------------|
| `Item` TPH | One `Items` table holds Weapons, Armor, Consumables, and KeyItems |
| `Container` TPH | One `Containers` table holds Inventories and Equipment (and later, Chests, MonsterLoot, Rooms) |
| `IItemContainer` | Interface every container implements — `Items`, `AddItem`, `RemoveItem` |
| `OfType<T>()` | LINQ method that filters a collection to a specific subtype |
| `GroupBy` | LINQ method that groups items by a property |
| `ICollection<T>` vs `IEnumerable<T>` vs `IQueryable<T>` | When to use each — see Tips section |
| Seed migration | A migration that runs a `.sql` script via `BaseMigration` |

---

## The Big Idea: Items Are Instances, Not Types

This is the mental model that makes everything click:

**If two players both have "a sword," that's TWO rows in the Items table.** Same `Name`, same `Attack`, different `Id`, different `ContainerId`. Items are *physical instances*, not categories.

This is why the relationship between `Container` and `Item` is **one-to-many**, not many-to-many:

- A container has many items ✓
- An item has exactly **one** container at any moment ✓

You can't hold the same literal sword in two places at once. Moving an item = changing one foreign key:

```csharp
// Pick up a sword from a chest
sword.ContainerId = player.Inventory.Id;
_context.SaveChanges();
```

That's it. One line changes the item's physical location.

> **Contrast with Week 10 Abilities:** abilities ARE types (knowledge/skills that multiple players can know simultaneously). That's why `Character <-> Ability` is many-to-many. Items are physical objects — many-to-one.

---

## Project Structure

```
W12-assignment-template.sln
│
├── ConsoleRpg/                           # UI & Game Logic
│   ├── Program.cs
│   ├── Startup.cs                        # DI + logging configuration
│   ├── appsettings.json                  # Connection string (single source of truth)
│   ├── Services/
│   │   └── GameEngine.cs                 # Main game loop & inventory menu (LINQ lives here!)
│   └── Helpers/
│       ├── MenuManager.cs                # Menu wrapper
│       └── OutputManager.cs              # Colored console output
│
└── ConsoleRpgEntities/                   # Data & Models
    ├── Data/
    │   ├── GameContext.cs                # DbSets + TPH config for Item and Container
    │   └── GameContextFactory.cs         # Design-time factory (for dotnet ef)
    ├── Models/
    │   ├── Characters/
    │   │   ├── Player.cs                 # Inventory + Equipment + combat + item methods
    │   │   └── Monsters/
    │   │       └── Goblin.cs
    │   ├── Containers/                   # NEW in W12
    │   │   ├── IItemContainer.cs         # Interface: Items, AddItem, RemoveItem
    │   │   ├── Container.cs              # Abstract base (TPH)
    │   │   ├── Inventory.cs              # Container subclass (backpack + MaxWeight)
    │   │   ├── Equipment.cs              # Container subclass (equipped slots)
    │   │   ├── Item.cs                   # Abstract base (TPH)
    │   │   ├── Weapon.cs                 # Item subclass (Attack, Category)
    │   │   ├── Armor.cs                  # Item subclass (Defense, Slot)
    │   │   ├── Consumable.cs             # Item subclass (EffectType, EffectAmount, Uses)
    │   │   └── KeyItem.cs                # Item subclass (KeyId)
    │   └── Abilities/                    # From W10
    ├── Helpers/
    │   ├── ConfigurationHelper.cs
    │   └── MigrationHelper.cs            # Loads .sql files for seed migrations
    └── Migrations/
        ├── BaseMigration.cs              # RunSql() / RunSqlRollback() helpers
        ├── 20260410182937_InitialCreate.cs
        ├── 20260410183100_SeedInitialData.cs
        └── Scripts/
            ├── SeedInitialData.sql       # Fresh starter player + items
            └── SeedInitialData.rollback.sql
```

---

## Assignment Tasks

### Task 1: Understand the Model Hierarchies

Open the `Models/Containers/` folder and read through the files in this order:

1. **`IItemContainer.cs`** — the minimal contract every container must satisfy
2. **`Container.cs`** — the TPH base class that implements `IItemContainer`
3. **`Inventory.cs` & `Equipment.cs`** — the two concrete container types for Week 12
4. **`Item.cs`** — the TPH base class for every physical object in the game
5. **`Weapon.cs`, `Armor.cs`, `Consumable.cs`, `KeyItem.cs`** — the four item subtypes

**Discussion prompt:** look at the Items table after running the migration. Notice all the nullable columns (`Attack`, `Defense`, `EffectType`, `KeyId`). Why are they nullable? Which rows have values for each?

### Task 2: Run the Migrations

From the solution directory:

```bash
dotnet ef database update --project ConsoleRpgEntities --startup-project ConsoleRpg
```

This runs two migrations in order:
1. **`InitialCreate`** — creates all the tables (Containers, Items, Players, Monsters, Abilities, PlayerAbilities)
2. **`SeedInitialData`** — runs `Migrations/Scripts/SeedInitialData.sql` to seed a starting player ("Elara the Bold"), her backpack full of items, and a goblin to fight

> **Tip:** Open SQL Server Object Explorer and look at the `Items` table. Notice the `ItemType` discriminator column. Notice how weapons have values for `Attack` and `Category`, but NULLs for `Defense`, `Slot`, `EffectType`, etc. That's TPH in action — one table, many types.

### Task 3: Explore the Player Methods

Open `Models/Characters/Player.cs` and trace through these methods:

- `PickUp(Item item)` — checks weight limit, adds to Inventory
- `Drop(Item item)` — removes from Inventory
- `Equip(Item item)` — moves an item from Inventory to Equipment
- `Unequip(Item item)` — moves an item from Equipment to Inventory
- `UseItem(Item item)` — applies a Consumable's effect and decrements `Uses`
- `GetTotalAttack()` / `GetTotalDefense()` — uses `OfType<Weapon>()` and `OfType<Armor>()` to sum equipped bonuses

Notice how `Equip` and `Unequip` are just "remove from one container, add to another." The database FK updates automatically when you call `SaveChanges()`.

### Task 4: Study the LINQ Queries

Open `Services/GameEngine.cs`. The inventory menu has five LINQ-heavy methods:

- **`ListItems()`** — simple iteration + `Sum` for total weight
- **`SearchByName()`** — `Where` with case-insensitive `Contains`
- **`GroupByType()`** — `GroupBy` on the discriminator column, with `OrderBy`
- **`SortSubmenu()`** — `OrderBy` / `OrderByDescending` driven by user input
- **`EquipItem()`** and **`UseConsumable()`** — use `OfType<T>()` to filter by subclass

Run the program, go into Inventory Management, and try each option against Elara's starter items.

### Task 5: Your Assignment — Extend the Inventory with YOUR LINQ

Your job is to add **two new inventory operations** using advanced LINQ:

**A. "Find the strongest weapon I own"**
Use `OfType<Weapon>()` and `OrderByDescending(w => w.Attack).First()`.
Display the weapon's name and Attack value. If there are no weapons, print a friendly message.

**B. "Total value of my inventory"**
Use `Sum(i => i.Value)` over the entire inventory.
Also show a breakdown by `ItemType` using `GroupBy` — how many gold pieces' worth of weapons, armor, consumables, etc.

Wire both into the Inventory menu as new options (8 and 9).

---

## LINQ Quick Reference

```csharp
// Where — filter by condition
var cheap = player.Inventory.Items.Where(i => i.Value < 50);

// OfType<T>() — filter to a specific subclass
var weapons = player.Inventory.Items.OfType<Weapon>();
var potions = player.Inventory.Items.OfType<Consumable>();

// GroupBy — group by a property (returns IEnumerable<IGrouping<TKey, TElement>>)
var groups = player.Inventory.Items.GroupBy(i => i.ItemType);
foreach (var group in groups)
{
    Console.WriteLine($"{group.Key}: {group.Count()} items");
}

// OrderBy / OrderByDescending
var sortedByName  = player.Inventory.Items.OrderBy(i => i.Name);
var heaviestFirst = player.Inventory.Items.OrderByDescending(i => i.Weight);

// Sum — totals over a numeric property
int totalValue  = player.Inventory.Items.Sum(i => i.Value);
int totalWeight = (int)player.Inventory.Items.Sum(i => i.Weight);

// Count — quick counts
int weaponCount = player.Inventory.Items.OfType<Weapon>().Count();

// Any — existence check
bool hasHealing = player.Inventory.Items.OfType<Consumable>().Any(c => c.EffectType == "Heal");

// Chaining
var topWeapons = player.Inventory.Items
    .OfType<Weapon>()
    .OrderByDescending(w => w.Attack)
    .Take(3)
    .ToList();
```

---

## Stretch Goal (+10%)

**Inventory Weight Limit Enforcement**

The `Inventory.MaxWeight` property is already defined, and `Player.PickUp()` already checks it when adding new items. Your stretch goal is to extend this:

1. Display the current weight as `12.5 / 100 lbs` in the inventory listing
2. When sorting, add a "Sort by Weight" option
3. Add a new menu option: **"Show items I could still pick up"** — take a list of hypothetical items (you can hardcode 5 for the demo) and use LINQ `Where` to return only those that would fit within the remaining weight capacity
4. Bonus: use LINQ `Aggregate` to find "the single heaviest combination of items that fits under MaxWeight" (this is the classic knapsack problem — any reasonable approximation is fine)

---

## Grading Rubric

| Criteria | Points | Description |
|----------|--------|-------------|
| Migration Runs Cleanly | 15 | `dotnet ef database update` succeeds with no errors |
| Understands Item/Container TPH | 15 | Can explain discriminator columns and WHY items use one-to-many |
| LINQ Task A: Strongest Weapon | 25 | `OfType<Weapon>` + `OrderByDescending` + graceful empty case |
| LINQ Task B: Total Value + Breakdown | 25 | `Sum` + `GroupBy` with a clean printout |
| Menu Integration | 10 | Both new options wired into the Inventory menu |
| Code Quality | 10 | Clean, readable, follows existing patterns |
| **Total** | **100** | |
| **Stretch: Weight Limit System** | **+10** | All four weight-related extensions above |

---

## How This Connects to Future Weeks

| Week | What gets added | The Container pattern lets us... |
|------|-----------------|----------------------------------|
| **W13** | `Chest` (lockable, trappable) and `MonsterLoot` (drops on defeat) as new Container subclasses | add new places-to-hold-items with ONE new class + ONE migration. No schema change. |
| **W14** | `Room` as a Container subclass (items on the floor) + full N/S/E/W navigation | drop items into rooms, walk into rooms, pick items off the floor, all with the same `AddItem`/`RemoveItem` API |

The interface you implement this week (`IItemContainer`) is the spine of the rest of the course. By W14, `container.AddItem(sword)` will work identically whether `container` is a backpack, a chest, a corpse, or a room floor.

---

## Tips

- **`ICollection<T>` vs `IEnumerable<T>` vs `IQueryable<T>`**:
  - Use `ICollection<T>` for navigation properties on entities (EF Core needs `.Add()`/`.Remove()`)
  - Use `IEnumerable<T>` for read-only in-memory iteration
  - Use `IQueryable<T>` when building queries that should be translated to SQL
- Use `ToList()` to force immediate evaluation of a LINQ query
- Remember `StringComparison.OrdinalIgnoreCase` for case-insensitive text search
- `OfType<T>()` is a LINQ method that filters AND casts in one step — perfect for TPH hierarchies
- When you modify an entity and want it saved, call `_context.SaveChanges()`

---

## Submission

1. Commit your changes with a meaningful message
2. Push to your GitHub Classroom repository
3. Submit the repository URL in Canvas

---

## Resources

- [LINQ GroupBy](https://learn.microsoft.com/en-us/dotnet/csharp/linq/group-query-results)
- [LINQ OrderBy](https://learn.microsoft.com/en-us/dotnet/csharp/linq/order-results-of-join-clause)
- [Enumerable.OfType<TResult>](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.oftype)
- [EF Core Inheritance (TPH)](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance)

---

## Need Help?

- Post questions in the Canvas discussion board
- Attend office hours
- Review the in-class repository for additional examples
