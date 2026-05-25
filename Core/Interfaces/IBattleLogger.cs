namespace Core.Interfaces
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

        void LogBuffAdded(IUnit squire, IUnit target, string buffName, bool isArmy1);
        void LogBuffLost(IUnit unit, string buffName, bool attackerIsArmy1);
        void LogCloneChance(IUnit wizard, int chancePercent, bool isArmy1);
        void LogCloneFailed(IUnit wizard, int newChancePercent, bool isArmy1);
        void LogCloneSuccess(IUnit wizard, string targetName, bool isArmy1);

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
        void LogHeal(IUnit healer, IUnit target, int healedAmount, bool healerIsArmy1);
        void LogHealNoEffect(IUnit healer, IUnit target, bool healerIsArmy1);
    }
}