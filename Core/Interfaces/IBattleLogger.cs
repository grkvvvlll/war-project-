namespace gaaameee.Core.Interfaces
{
    public interface IBattleLogger
    {
        void Log(string message);
        void LogSpecial(IUnit user, IUnit target, string abilityName, int damage);
        void LogInfo(string message);

        void LogHit(
            IUnit attacker,
            IUnit defender,
            int damage,
            int oldHp,
            bool attackerIsArmy1);

        void LogDeath(IUnit unit, bool isArmy1);

        void LogArcherShot(
            IUnit archer,
            int range,
            int distance,
            bool isArmy1);

        void LogArrowMiss();

        void LogArcherHit(
            IUnit archer,
            IUnit target,
            int oldHp,
            int newHp,
            bool isArmy1);

        void LogNoArchers(string armyName);
    }
}