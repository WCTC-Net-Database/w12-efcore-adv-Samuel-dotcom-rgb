namespace ConsoleRpgEntities.Models.Containers;

/// <summary>
/// Container - Abstract base class for anything that can hold items in the database.
///
/// =======================================================
/// TPH (Table-Per-Hierarchy) - Container Hierarchy
/// =======================================================
/// This class uses the same TPH pattern you learned in Week 10 for Monsters and Abilities.
/// All containers live in ONE "Containers" table with a "ContainerType" discriminator column.
///
/// Why Container as a base class?
/// - Every container has an Id and a collection of items
/// - Every container behaves the same way when holding, adding, or removing items
/// - Future container types (Chest, MonsterLoot, Room) just add a new subclass - no schema changes
///
/// Why is this better than separate tables for Inventory, Equipment, Chest, etc.?
/// - Items only need ONE foreign key (ContainerId) instead of one per container type
/// - Moving an item between containers is a single line: item.ContainerId = newContainer.Id;
/// - Polymorphic queries work: _context.Containers.OfType&lt;Chest&gt;() returns only chests
/// </summary>
public abstract class Container : IItemContainer
{
    public int Id { get; set; }

    /// <summary>
    /// Discriminator column set automatically by EF Core based on the concrete type.
    /// Used to identify which subclass (Inventory, Equipment, Chest, etc.) a row represents.
    /// </summary>
    public string ContainerType { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for items in this container.
    /// This is a one-to-many relationship: one Container has many Items.
    /// Each Item has exactly one Container (Item.ContainerId is its single FK).
    /// </summary>
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    /// <summary>
    /// Adds an item to this container and updates the item's ContainerId.
    /// Call _context.SaveChanges() after this to persist to the database.
    /// </summary>
    public virtual void AddItem(Item item)
    {
        item.ContainerId = Id;
        Items.Add(item);
    }

    /// <summary>
    /// Removes an item from this container. Returns true if the item was found and removed.
    /// Note: this only detaches the item from THIS container. The caller is responsible for
    /// placing it into a new container (otherwise EF Core will fail validation).
    /// </summary>
    public virtual bool RemoveItem(Item item)
    {
        return Items.Remove(item);
    }
}
