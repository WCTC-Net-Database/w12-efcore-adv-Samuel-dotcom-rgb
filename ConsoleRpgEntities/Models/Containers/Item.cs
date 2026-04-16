using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// Item - Abstract base class for every physical object in the game world.
///
/// =======================================================
/// TPH (Table-Per-Hierarchy) - Item Hierarchy
/// =======================================================
/// Every item (weapon, armor, potion, key) lives in ONE "Items" table with an
/// "ItemType" discriminator column. You already used this pattern for Monsters (W10).
///
/// Hierarchy:
///   Item (abstract)
///   ├── Weapon     (Attack damage, optional magical/physical type)
///   ├── Armor      (Defense rating, slot type)
///   ├── Consumable (heal potions, mana potions - Uses property for quantity)
///   └── KeyItem    (quest items, lockpicks - no combat stats)
///
/// =======================================================
/// One item, one container (one-to-many, NOT many-to-many)
/// =======================================================
/// Every physical item has ONE current location: ContainerId points at a Container row.
/// Moving an item = changing ContainerId. That's it.
///
/// Two players each carrying "a sword" means TWO rows in the Items table, not one row
/// referenced twice. Items are INSTANCES, not TYPES.
///
/// Compare this to Week 10 Abilities (many-to-many): an ability like "Fireball" can be
/// KNOWN by multiple characters at once because abilities are knowledge/skills.
/// Items are physical objects - you can't hold the same literal sword in two places.
/// </summary>
public abstract class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discriminator column set automatically by EF Core based on the concrete type.
    /// Values: "Weapon", "Armor", "Consumable", "KeyItem".
    /// </summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>
    /// Weight in inventory (used by the stretch goal weight-limit system).
    /// </summary>
    [Column(TypeName = "decimal(6, 2)")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Trade/sell value in gold.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Foreign key to the container this item currently lives in.
    /// Nullable during construction before the item is placed anywhere.
    /// </summary>
    public int? ContainerId { get; set; }
    public virtual Container? Container { get; set; }
}
