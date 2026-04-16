namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// Inventory - A player's backpack. Holds items the player is carrying but not using.
///
/// This is a Container subclass stored in the shared Containers table with
/// ContainerType = "Inventory". The link back to the owning Player is on
/// Player.InventoryId (one-to-one), so no back-reference is needed here.
/// </summary>
public class Inventory : Container
{
    /// <summary>
    /// Stretch goal support: maximum carrying weight in the backpack.
    /// </summary>
    public int MaxWeight { get; set; } = 100;
}
