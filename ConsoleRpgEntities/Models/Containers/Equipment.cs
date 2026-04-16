namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// Equipment - A player's equipped item slots (weapon, armor, etc.).
///
/// This is a Container subclass - all equipped items live in the same Items table
/// with ContainerId pointing at this Equipment row. Querying "what's equipped" is:
///
///     var weapon = player.Equipment.Items.OfType&lt;Weapon&gt;().FirstOrDefault();
///     var armor  = player.Equipment.Items.OfType&lt;Armor&gt;().FirstOrDefault();
///
/// This is much simpler than Week 11's separate WeaponId/ArmorId foreign keys.
/// </summary>
public class Equipment : Container
{
}
