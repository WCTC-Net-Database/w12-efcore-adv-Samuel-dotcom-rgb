namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// Consumable - Items that produce an effect when used (potions, food, scrolls).
/// Stored in the Items table with ItemType = "Consumable".
///
/// When a consumable is used, the effect is applied and Uses is decremented.
/// When Uses reaches 0, the item should be removed from its container.
/// </summary>
public class Consumable : Item
{
    /// <summary>
    /// What the consumable does: "Heal", "Mana", "Buff", etc.
    /// </summary>
    public string EffectType { get; set; } = "Heal";

    /// <summary>
    /// How much the effect does (HP restored, damage increased, etc.).
    /// </summary>
    public int EffectAmount { get; set; }

    /// <summary>
    /// Remaining uses. Defaults to 1 for single-use potions.
    /// </summary>
    public int Uses { get; set; } = 1;
}
