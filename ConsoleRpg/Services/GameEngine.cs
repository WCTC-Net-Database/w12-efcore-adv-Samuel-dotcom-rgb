using ConsoleRpg.Helpers;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models.Containers;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpg.Services;

/// <summary>
/// GameEngine - Week 12 entry point for inventory management.
///
/// The Run() method loads the player (with Inventory, Equipment, and Items eager-loaded)
/// then delegates to MenuManager for the main loop. All inventory operations happen
/// through the Player's container methods (PickUp, Drop, Equip, Unequip, UseItem).
/// </summary>
public class GameEngine
{
    private readonly GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private Player? _player;
    private Goblin? _goblin;

    public GameEngine(GameContext context, MenuManager menuManager, OutputManager outputManager)
    {
        _menuManager = menuManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (!_menuManager.ShowMainMenu())
            return;

        SetupGame();
        if (_player == null)
        {
            _outputManager.WriteLine("No player found in the database. Run the seed migration first.", ConsoleColor.Red);
            _outputManager.Display();
            return;
        }

        GameLoop();
    }

    private void SetupGame()
    {
        // Eager-load everything the player touches: containers and their items.
        // Without these .Include calls, Inventory.Items and Equipment.Items would be empty.
        _player = _context.Players
            .Include(p => p.Inventory!)
                .ThenInclude(i => i.Items)
            .Include(p => p.Equipment!)
                .ThenInclude(e => e.Items)
            .Include(p => p.Abilities)
            .FirstOrDefault();

        _goblin = _context.Monsters.OfType<Goblin>().FirstOrDefault();

        if (_player != null)
        {
            _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);
            _outputManager.Display();
            Thread.Sleep(400);
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Inventory Management");
            _outputManager.WriteLine("2. Attack Monster");
            _outputManager.WriteLine("3. Quit");
            _outputManager.Display();

            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    InventoryMenu();
                    break;
                case "2":
                    AttackCharacter();
                    break;
                case "3":
                    return;
                default:
                    _outputManager.WriteLine("Invalid selection.", ConsoleColor.Red);
                    _outputManager.Display();
                    Thread.Sleep(600);
                    break;
            }
        }
    }

    // ============================================================
    // INVENTORY MENU (Week 12 - LINQ Focus)
    // ============================================================
    private void InventoryMenu()
    {
        if (_player?.Inventory == null)
            return;

        while (true)
        {
            _outputManager.Clear();
            _outputManager.WriteLine("Inventory Management", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. List all items");
            _outputManager.WriteLine("2. Search by name");
            _outputManager.WriteLine("3. Group by type");
            _outputManager.WriteLine("4. Sort items");
            _outputManager.WriteLine("5. Equip item");
            _outputManager.WriteLine("6. Use consumable");
            _outputManager.WriteLine("7. Drop item");
            _outputManager.WriteLine("8. Find strongest weapon");
            _outputManager.WriteLine("9. Calculate total inventory value");
            _outputManager.WriteLine("10. Show items I could still pick up");
            _outputManager.WriteLine("0. Back");
            _outputManager.Display();

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListItems(); break;
                case "2": SearchByName(); break;
                case "3": GroupByType(); break;
                case "4": SortSubmenu(); break;
                case "5": EquipItem(); break;
                case "6": UseConsumable(); break;
                case "7": DropItem(); break;
                case "8": FindStrongestWeapon(); break;
                case "9": CalculateInventoryValue(); break;
                case "10": ShowPickupableItems(); break;
                case "0": return;
                default:
                    _outputManager.WriteLine("Invalid selection.", ConsoleColor.Red);
                    _outputManager.Display();
                    Thread.Sleep(600);
                    break;
            }
        }
    }

    private void ListItems()
    {
        if (_player?.Inventory == null) return;

        // Stretch Goal: Display weight in X.X / MaxWeight format
        decimal currentWeight = _player.Inventory.Items.Sum(i => i.Weight);
        Console.WriteLine($"\n{_player.Name}'s backpack ({currentWeight}/{_player.Inventory.MaxWeight} lbs):");
        if (!_player.Inventory.Items.Any())
        {
            Console.WriteLine("  (empty)");
        }
        else
        {
            foreach (var item in _player.Inventory.Items)
            {
                Console.WriteLine($"  - {item.Name} [{item.ItemType}] ({item.Weight} lbs, {item.Value}g)");
            }
        }
        Pause();
    }

    private void SearchByName()
    {
        if (_player?.Inventory == null) return;

        Console.Write("\nEnter search term: ");
        var term = Console.ReadLine() ?? string.Empty;

        // LINQ: Where + case-insensitive Contains
        var results = _player.Inventory.Items
            .Where(i => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (results.Any())
        {
            Console.WriteLine($"\nFound {results.Count} item(s):");
            foreach (var item in results)
                Console.WriteLine($"  - {item.Name} [{item.ItemType}]");
        }
        else
        {
            Console.WriteLine("No matching items.");
        }
        Pause();
    }

    private void GroupByType()
    {
        if (_player?.Inventory == null) return;

        // LINQ: GroupBy
        var groups = _player.Inventory.Items
            .GroupBy(i => i.ItemType)
            .OrderBy(g => g.Key);

        Console.WriteLine();
        foreach (var group in groups)
        {
            Console.WriteLine($"{group.Key} ({group.Count()}):");
            foreach (var item in group)
                Console.WriteLine($"  - {item.Name}");
        }
        Pause();
    }

    private void SortSubmenu()
    {
        if (_player?.Inventory == null) return;

        Console.WriteLine("\nSort by:");
        Console.WriteLine("1. Name");
        Console.WriteLine("2. Weight (heaviest first)");
        Console.WriteLine("3. Value (most valuable first)");
        Console.WriteLine("4. Weight (lightest first)");
        Console.Write("Choice: ");
        var choice = Console.ReadLine();

        // LINQ: OrderBy / OrderByDescending
        var sorted = choice switch
        {
            "1" => _player.Inventory.Items.OrderBy(i => i.Name).ToList(),
            "2" => _player.Inventory.Items.OrderByDescending(i => i.Weight).ToList(),
            "3" => _player.Inventory.Items.OrderByDescending(i => i.Value).ToList(),
            "4" => _player.Inventory.Items.OrderBy(i => i.Weight).ToList(),
            _ => _player.Inventory.Items.ToList()
        };

        Console.WriteLine();
        foreach (var item in sorted)
            Console.WriteLine($"  {item.Name,-30} {item.Weight,5} lbs  {item.Value,5}g");
        Pause();
    }

    private void EquipItem()
    {
        if (_player?.Inventory == null) return;

        var equippable = _player.Inventory.Items
            .Where(i => i is Weapon || i is Armor)
            .ToList();

        if (!equippable.Any())
        {
            Console.WriteLine("Nothing equippable in your backpack.");
            Pause();
            return;
        }

        Console.WriteLine();
        for (int i = 0; i < equippable.Count; i++)
            Console.WriteLine($"  {i + 1}. {equippable[i].Name}");

        Console.Write("Which item? ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= equippable.Count)
        {
            _player.Equip(equippable[idx - 1]);
            _context.SaveChanges();
        }
        Pause();
    }

    private void UseConsumable()
    {
        if (_player?.Inventory == null) return;

        var consumables = _player.Inventory.Items.OfType<Consumable>().ToList();
        if (!consumables.Any())
        {
            Console.WriteLine("No consumables in your backpack.");
            Pause();
            return;
        }

        Console.WriteLine();
        for (int i = 0; i < consumables.Count; i++)
            Console.WriteLine($"  {i + 1}. {consumables[i].Name} ({consumables[i].EffectType} {consumables[i].EffectAmount})");

        Console.Write("Which item? ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= consumables.Count)
        {
            _player.UseItem(consumables[idx - 1]);
            _context.SaveChanges();
        }
        Pause();
    }

    private void DropItem()
    {
        if (_player?.Inventory == null) return;

        var items = _player.Inventory.Items.ToList();
        if (!items.Any())
        {
            Console.WriteLine("Backpack is empty.");
            Pause();
            return;
        }

        Console.WriteLine();
        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"  {i + 1}. {items[i].Name}");

        Console.Write("Which item? ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= items.Count)
        {
            var item = items[idx - 1];
            _player.Drop(item);
            // In W12, dropped items are orphaned (ContainerId = null).
            // In W14 we'll place them into the current Room's container instead.
            _context.Items.Remove(item);
            _context.SaveChanges();
        }
        Pause();
    }

    // ============================================================
    // TASK 5A: FIND STRONGEST WEAPON (Advanced LINQ)
    // ============================================================
    /// <summary>
    /// Finds the weapon with the highest Attack value in the inventory.
    /// Uses OfType<Weapon>() to filter to weapons, then OrderByDescending to find the strongest.
    /// </summary>
    private void FindStrongestWeapon()
    {
        if (_player?.Inventory == null) return;

        // LINQ: OfType<T>() + OrderByDescending + First
        var strongestWeapon = _player.Inventory.Items
            .OfType<Weapon>()
            .OrderByDescending(w => w.Attack)
            .FirstOrDefault();

        Console.WriteLine();
        if (strongestWeapon != null)
        {
            Console.WriteLine($"Your strongest weapon: {strongestWeapon.Name}");
            Console.WriteLine($"  Attack Power: {strongestWeapon.Attack}");
            Console.WriteLine($"  Category: {strongestWeapon.Category}");
            Console.WriteLine($"  Weight: {strongestWeapon.Weight} lbs");
            Console.WriteLine($"  Value: {strongestWeapon.Value}g");
        }
        else
        {
            Console.WriteLine("You don't have any weapons in your backpack.");
        }
        Pause();
    }

    // ============================================================
    // TASK 5B: CALCULATE INVENTORY VALUE (Advanced LINQ with GroupBy)
    // ============================================================
    /// <summary>
    /// Calculates the total value of the inventory and shows a breakdown by ItemType using GroupBy.
    /// Demonstrates Sum and GroupBy LINQ operations.
    /// </summary>
    private void CalculateInventoryValue()
    {
        if (_player?.Inventory == null) return;

        Console.WriteLine();
        if (!_player.Inventory.Items.Any())
        {
            Console.WriteLine("Your backpack is empty.");
            Pause();
            return;
        }

        // LINQ: Sum for total value
        int totalValue = _player.Inventory.Items.Sum(i => i.Value);

        Console.WriteLine($"Total Inventory Value: {totalValue}g");
        Console.WriteLine();
        Console.WriteLine("Value Breakdown by Item Type:");

        // LINQ: GroupBy on ItemType, ordered by key
        var valueByType = _player.Inventory.Items
            .GroupBy(i => i.ItemType)
            .OrderBy(g => g.Key);

        foreach (var group in valueByType)
        {
            int typeValue = group.Sum(i => i.Value);
            Console.WriteLine($"  {group.Key,-15} : {typeValue,6}g ({group.Count()} item{(group.Count() != 1 ? "s" : "")})");
        }

        Pause();
    }

    // ============================================================
    // STRETCH GOAL: SHOW ITEMS I COULD STILL PICK UP
    // ============================================================
    /// <summary>
    /// Stretch goal: Demonstrates weight limit enforcement and LINQ Where filtering.
    /// Shows a list of hypothetical items and uses LINQ to determine which ones fit
    /// within the remaining weight capacity.
    /// </summary>
    private void ShowPickupableItems()
    {
        if (_player?.Inventory == null) return;

        // Calculate remaining weight capacity
        decimal currentWeight = _player.Inventory.Items.Sum(i => i.Weight);
        decimal remainingWeight = _player.Inventory.MaxWeight - currentWeight;

        Console.WriteLine();
        Console.WriteLine($"Current Weight: {currentWeight}/{_player.Inventory.MaxWeight} lbs");
        Console.WriteLine($"Remaining Capacity: {remainingWeight} lbs");
        Console.WriteLine();

        // Create a list of hypothetical items that might be found in the game world
        var availableItems = new List<(string Name, decimal Weight, int Value)>
        {
            ("Ancient Tome", 3.5m, 250),
            ("Healing Potion", 0.5m, 50),
            ("Iron Ingot", 5.0m, 100),
            ("Silk Scroll", 0.25m, 150),
            ("Diamond Ring", 0.1m, 500),
            ("Copper Coin", 0.01m, 1),
            ("Leather Armor", 8.0m, 200),
            ("Wooden Staff", 4.0m, 180),
            ("Silver Dagger", 2.0m, 300),
            ("Rope Coil", 2.5m, 25)
        };

        // LINQ: Where filter to show only items that fit in remaining capacity
        var pickupable = availableItems
            .Where(item => item.Weight <= remainingWeight)
            .OrderBy(item => item.Weight)
            .ToList();

        if (pickupable.Any())
        {
            Console.WriteLine($"Items you could pick up ({pickupable.Count()} available):");
            foreach (var item in pickupable)
            {
                Console.WriteLine($"  - {item.Name,-20} : {item.Weight,5} lbs  {item.Value,5}g");
            }

            // Optional Bonus: Knapsack approximation using Aggregate
            Console.WriteLine();
            Console.WriteLine("Knapsack Optimization (greedy by value/weight ratio):");

            // LINQ: Aggregate to build the best combination
            var ratios = pickupable.Select(item => new
            {
                Name = item.Name,
                Weight = item.Weight,
                Value = item.Value,
                Ratio = item.Value / item.Weight
            }).OrderByDescending(x => x.Ratio).ToList();

            var knapsack = new { TotalWeight = 0m, TotalValue = 0, Items = new List<string>() };

            knapsack = ratios.Aggregate(knapsack, (acc, item) =>
            {
                if (acc.TotalWeight + item.Weight <= remainingWeight)
                {
                    acc.Items.Add(item.Name);
                    return new
                    {
                        TotalWeight = acc.TotalWeight + item.Weight,
                        TotalValue = acc.TotalValue + item.Value,
                        Items = acc.Items
                    };
                }
                return acc;
            });

            Console.WriteLine($"  Best combination: {knapsack.TotalValue}g in {knapsack.TotalWeight} lbs");
            if (knapsack.Items.Any())
            {
                foreach (var item in knapsack.Items)
                {
                    Console.WriteLine($"    - {item}");
                }
            }
        }
        else
        {
            Console.WriteLine("You're carrying too much - no more items can fit!");
        }

        Pause();
    }

    // ============================================================
    // COMBAT (simplified for W12 focus)
    // ============================================================
    private void AttackCharacter()
    {
        if (_player == null || _goblin == null)
        {
            Console.WriteLine("No target available.");
            Pause();
            return;
        }

        _player.Attack(_goblin);
        Pause();
    }

    private void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}
