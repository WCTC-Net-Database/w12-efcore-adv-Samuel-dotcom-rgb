using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;

namespace ConsoleRpgEntities.Models.Abilities.PlayerAbilities
{
    public class ShoveAbility : Ability
    {
        public int Damage { get; set; }
        public int Distance { get; set; }

        public override void Activate(IPlayer user, ITargetable target)
        {
            // Fireball ability logic
            Console.WriteLine($"{user.Name} shoves {target.Name} back {Distance} feet, dealing {Damage} damage!");
        }
    }
}
