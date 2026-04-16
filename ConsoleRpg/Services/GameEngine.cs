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

        Console.WriteLine($"\n{_player.Name}'s backpack ({_player.GetCurrentWeight()}/{_player.Inventory.MaxWeight} lbs):");
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
        Console.Write("Choice: ");
        var choice = Console.ReadLine();

        // LINQ: OrderBy / OrderByDescending
        var sorted = choice switch
        {
            "1" => _player.Inventory.Items.OrderBy(i => i.Name).ToList(),
            "2" => _player.Inventory.Items.OrderByDescending(i => i.Weight).ToList(),
            "3" => _player.Inventory.Items.OrderByDescending(i => i.Value).ToList(),
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
