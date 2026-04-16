namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// Armor - Items that reduce incoming damage.
/// Stored in the Items table with ItemType = "Armor".
/// </summary>
public class Armor : Item
{
    public int Defense { get; set; }

    /// <summary>
    /// Slot the armor occupies when equipped (Head, Body, Hands, Feet, etc.).
    /// Used later to prevent equipping two chestpieces at the same time.
    /// </summary>
    public string Slot { get; set; } = "Body";
}
