using ConsoleRpgEntities.Models.Attributes;

namespace ConsoleRpgEntities.Models.Characters.Monsters
{
    public abstract class Monster : IMonster, ITargetable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Health { get; set; }
        public int AggressionLevel { get; set; }
        public string MonsterType { get; set; } = string.Empty;

        protected Monster()
        {

        }

        public abstract void Attack(ITargetable target);

    }
}
