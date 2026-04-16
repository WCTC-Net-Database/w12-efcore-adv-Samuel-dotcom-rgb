namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// KeyItem - Quest items, lockpicks, keys, and other utility items with no combat stats.
/// Stored in the Items table with ItemType = "KeyItem".
///
/// In Week 13 these will interact with Chests (locked chests need a matching key or lockpick).
/// </summary>
public class KeyItem : Item
{
    /// <summary>
    /// Optional tag that links this key to a specific lock (e.g., "dungeon-main", "goblin-cage").
    /// Null for generic utility items like lockpicks.
    /// </summary>
    public string? KeyId { get; set; }
}
