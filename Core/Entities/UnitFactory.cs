using gaaameee.Core.Entities;

namespace gaaameee.Core.Factories
{
    public static class UnitFactory
    {
        // ===== HEAVY =====
        public const int HeavyAttack = 8;
        public const int HeavyDefence = 6;
        public const int HeavyHP = 25;
        public const int HeavyCost = 50;

        // ===== LIGHT =====
        public const int LightAttack = 10;
        public const int LightDefence = 3;
        public const int LightHP = 15;
        public const int LightCost = 30;

        // ===== ARCHER =====
        public const int ArcherAttack = 7;
        public const int ArcherDefence = 2;
        public const int ArcherHP = 12;
        public const int ArcherCost = 40;
        public const int ArcherRange = 5;

        public static HeavyUnit CreateHeavy(string name)
        {
            return new HeavyUnit(
                name,
                HeavyAttack,
                HeavyDefence,
                HeavyHP,
                HeavyCost);
        }

        public static LightUnit CreateLight(string name)
        {
            return new LightUnit(
                name,
                LightAttack,
                LightDefence,
                LightHP,
                LightCost);
        }

        public static Archer CreateArcher(string name)
        {
            return new Archer(
                name,
                ArcherAttack,
                ArcherDefence,
                ArcherHP,
                ArcherCost,
                ArcherRange);
        }
    }
}