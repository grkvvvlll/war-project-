namespace gaaameee.Core.Interfaces
{
    public interface IBattleLogger
    {
        void Log(string message);
        void LogHit(IUnit attacker, IUnit defender, int damage);
        void LogSpecial(IUnit user, IUnit target, string abilityName, int damage);
        void LogDeath(IUnit unit);
        void LogInfo(string message);
    }
}