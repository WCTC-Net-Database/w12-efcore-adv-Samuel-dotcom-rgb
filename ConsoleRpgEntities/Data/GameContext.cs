using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models.Containers;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpgEntities.Data;

/// <summary>
/// GameContext - EF Core database context for the ConsoleRPG game.
///
/// Week 12 adds two new entity hierarchies, both using TPH:
///   1. Container  → Inventory, Equipment (and later Chest, MonsterLoot, Room)
///   2. Item       → Weapon, Armor, Consumable, KeyItem
///
/// Items reference their current location with a single ContainerId foreign key.
/// That's all it takes to move an item between containers - one FK update.
/// </summary>
public class GameContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Monster> Monsters { get; set; }
    public DbSet<Ability> Abilities { get; set; }

    // New in Week 12
    public DbSet<Container> Containers { get; set; }
    public DbSet<Item> Items { get; set; }

    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ============================================
        // TPH: Monster hierarchy (from Week 10)
        // ============================================
        modelBuilder.Entity<Monster>()
            .HasDiscriminator<string>(m => m.MonsterType)
            .HasValue<Goblin>("Goblin");

        // ============================================
        // TPH: Ability hierarchy (from Week 10)
        // ============================================
        modelBuilder.Entity<Ability>()
            .HasDiscriminator<string>(a => a.AbilityType)
            .HasValue<ShoveAbility>("ShoveAbility");

        // Many-to-many: Player <-> Ability
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Abilities)
            .WithMany(a => a.Players)
            .UsingEntity(j => j.ToTable("PlayerAbilities"));

        // ============================================
        // TPH: Container hierarchy (NEW in Week 12)
        // ============================================
        // All containers (Inventory, Equipment, and later Chest/MonsterLoot/Room)
        // live in ONE "Containers" table with a ContainerType discriminator.
        modelBuilder.Entity<Container>()
            .HasDiscriminator<string>(c => c.ContainerType)
            .HasValue<Inventory>("Inventory")
            .HasValue<Equipment>("Equipment");

        // ============================================
        // TPH: Item hierarchy (NEW in Week 12)
        // ============================================
        // All items (Weapons, Armor, Consumables, KeyItems) live in ONE "Items"
        // table with an ItemType discriminator.
        modelBuilder.Entity<Item>()
            .HasDiscriminator<string>(i => i.ItemType)
            .HasValue<Weapon>("Weapon")
            .HasValue<Armor>("Armor")
            .HasValue<Consumable>("Consumable")
            .HasValue<KeyItem>("KeyItem");

        // ============================================
        // Container <-> Item relationship (one-to-many)
        // ============================================
        // Each item has exactly ONE current container. Each container has many items.
        // This is NOT many-to-many - an item can't be in two containers at once.
        // See the README for a discussion of why items are instances, not types.
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Container)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.ContainerId)
            .OnDelete(DeleteBehavior.SetNull);

        // ============================================
        // Player -> Inventory / Equipment (one-way)
        // ============================================
        // Player holds the FKs; Inventory and Equipment don't back-reference the Player.
        // This avoids a duplicate PlayerId column in the TPH Containers table.
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Inventory)
            .WithMany()
            .HasForeignKey(p => p.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.Equipment)
            .WithMany()
            .HasForeignKey(p => p.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
