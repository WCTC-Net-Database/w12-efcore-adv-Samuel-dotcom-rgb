using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Containers;

namespace ConsoleRpgEntities.Models.Characters;

/// <summary>
/// Player - The user's character. Holds references to two Containers:
///   - Inventory: backpack of items being carried
///   - Equipment: slots for currently equipped items
///
/// All inventory operations in Week 12 revolve around these two containers.
/// In Week 13 we'll add chests and monster loot as additional container types.
/// In Week 14 we'll add rooms as containers too, so items can exist on the ground.
/// </summary>
public class Player : ITargetable, IPlayer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int Health { get; set; } = 100;

    // ============================================================
    // CONTAINERS
    // ============================================================
    // A Player has exactly one Inventory and one Equipment (one-to-one each).
    // Both are stored in the shared Containers table (TPH).
    public int? InventoryId { get; set; }
    public virtual Inventory? Inventory { get; set; }

    public int? EquipmentId { get; set; }
    public virtual Equipment? Equipment { get; set; }

    // Many-to-many: Player can know many Abilities, each Ability known by many Players.
    public virtual ICollection<Ability> Abilities { get; set; } = new List<Ability>();

    // ============================================================
    // COMBAT
    // ============================================================
    public int GetTotalAttack()
    {
        int baseAttack = Level * 2;
        int weaponBonus = Equipment?.Items.OfType<Weapon>().Sum(w => w.Attack) ?? 0;
        return baseAttack + weaponBonus;
    }

    public int GetTotalDefense()
    {
        int baseDefense = Level;
        int armorBonus = Equipment?.Items.OfType<Armor>().Sum(a => a.Defense) ?? 0;
        return baseDefense + armorBonus;
    }

    public void Attack(ITargetable target)
    {
        int damage = GetTotalAttack();
        target.Health -= damage;
        Console.WriteLine($"{Name} attacks {target.Name} for {damage} damage!");
    }

    public void UseAbility(IAbility ability, ITargetable target)
    {
        if (Abilities.Any(a => a.Id == ability.Id))
        {
            ability.Activate(this, target);
        }
        else
        {
            Console.WriteLine($"{Name} does not know {ability.Name}!");
        }
    }

    // ============================================================
    // INVENTORY OPERATIONS
    // ============================================================
    // These methods mutate the in-memory graph. Call _context.SaveChanges()
    // after any of these to persist the changes to the database.

    /// <summary>
    /// Adds an item to the backpack. Returns false if the weight limit would be exceeded.
    /// </summary>
    public bool PickUp(Item item)
    {
        if (Inventory == null)
            return false;

        int newWeight = GetCurrentWeight() + (int)item.Weight;
        if (newWeight > Inventory.MaxWeight)
        {
            Console.WriteLine($"Cannot carry {item.Name} - too heavy! ({newWeight}/{Inventory.MaxWeight})");
            return false;
        }

        Inventory.AddItem(item);
        Console.WriteLine($"{Name} picked up {item.Name}.");
        return true;
    }

    /// <summary>
    /// Drops an item from the backpack. In W12 this just removes it.
    /// In W14 the item will be placed in the current room's container instead.
    /// </summary>
    public void Drop(Item item)
    {
        if (Inventory == null) return;

        if (Inventory.RemoveItem(item))
        {
            Console.WriteLine($"{Name} dropped {item.Name}.");
        }
    }

    /// <summary>
    /// Moves an item from inventory into an equipment slot.
    /// </summary>
    public void Equip(Item item)
    {
        if (Inventory == null || Equipment == null) return;

        if (!Inventory.Items.Contains(item))
        {
            Console.WriteLine($"{item.Name} isn't in the backpack.");
            return;
        }

        if (item is not Weapon && item is not Armor)
        {
            Console.WriteLine($"{item.Name} can't be equipped.");
            return;
        }

        Inventory.RemoveItem(item);
        Equipment.AddItem(item);
        Console.WriteLine($"{Name} equipped {item.Name}.");
    }

    /// <summary>
    /// Moves an item from an equipment slot back into the inventory.
    /// </summary>
    public void Unequip(Item item)
    {
        if (Inventory == null || Equipment == null) return;

        if (!Equipment.Items.Contains(item))
        {
            Console.WriteLine($"{item.Name} isn't equipped.");
            return;
        }

        Equipment.RemoveItem(item);
        Inventory.AddItem(item);
        Console.WriteLine($"{Name} unequipped {item.Name}.");
    }

    /// <summary>
    /// Uses a consumable item (potion, food, scroll). Decrements Uses and
    /// removes the item from the backpack when depleted.
    /// </summary>
    public void UseItem(Item item)
    {
        if (item is not Consumable consumable)
        {
            Console.WriteLine($"{item.Name} is not usable.");
            return;
        }

        switch (consumable.EffectType)
        {
            case "Heal":
                Health += consumable.EffectAmount;
                Console.WriteLine($"{Name} restored {consumable.EffectAmount} HP (now {Health}).");
                break;
            default:
                Console.WriteLine($"{Name} used {consumable.Name}.");
                break;
        }

        consumable.Uses--;
        if (consumable.Uses <= 0)
        {
            Inventory?.RemoveItem(consumable);
        }
    }

    // ============================================================
    // WEIGHT (stretch goal support)
    // ============================================================
    public int GetCurrentWeight()
    {
        return (int)(Inventory?.Items.Sum(i => i.Weight) ?? 0);
    }
}
