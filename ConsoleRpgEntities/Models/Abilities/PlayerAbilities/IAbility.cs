using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;

namespace ConsoleRpgEntities.Models.Abilities.PlayerAbilities;

public interface IAbility
{
    int Id { get; set; }
    string Name { get; set; }
    ICollection<Player> Players { get; set; }

    void Activate(IPlayer user, ITargetable target);
}