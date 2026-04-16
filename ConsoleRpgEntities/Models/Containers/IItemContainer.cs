namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// IItemContainer - A contract for anything that can hold items.
///
/// This interface is intentionally minimal: any container, regardless of type,
/// must be able to report what it holds, accept new items, and remove items.
///
/// In W12 this is implemented by Inventory and Equipment (both Container subclasses).
/// In W13 this will expand to Chest and MonsterLoot.
/// In W14 Room will also implement this, so items can exist "on the floor" of a room.
///
/// This is an example of the Interface Segregation Principle (ISP):
/// a container ONLY knows how to hold items. It doesn't know about locks,
/// sizes, owners, or locations - those are separate concerns handled elsewhere.
/// </summary>
public interface IItemContainer
{
    ICollection<Item> Items { get; }
    void AddItem(Item item);
    bool RemoveItem(Item item);
}
