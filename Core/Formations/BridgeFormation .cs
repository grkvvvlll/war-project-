using Core.Interfaces;

namespace Core.Formations
{
    public class BridgeFormation : IBattleFormation
    {
        public string Name => "Бой на мосту";
        public string Description => "Одна колонна. Юниты выстроены друг за другом.";

        public (int row, int col) GetPosition(IArmy army, int unitIndex, bool isArmy1)
        {
            int frontIndex = GetFrontIndex(army, isArmy1);
            if (frontIndex == -1) return (0, 0);
            int distToFront = CountAliveBetween(army, unitIndex, frontIndex, isArmy1);
            return (0, distToFront);
        }

        public bool IsOnFrontLine(IArmy army, int unitIndex, bool isArmy1)
        {
            return unitIndex == GetFrontIndex(army, isArmy1);
        }

        public bool CanUseSpecialAbility(IArmy myArmy, int unitIndex, IArmy enemyArmy, bool isArmy1)
        {
            return !IsOnFrontLine(myArmy, unitIndex, isArmy1);
        }

        public IUnit? GetMeleeAttacker(IArmy attackerArmy, bool attackerIsArmy1)
        {
            return attackerIsArmy1
                ? attackerArmy.Units.LastOrDefault(u => u.IsAlive)
                : attackerArmy.Units.FirstOrDefault(u => u.IsAlive);
        }

        public IUnit? GetMeleeDefender(IArmy defenderArmy, bool attackerIsArmy1)
        {
            return attackerIsArmy1
                ? defenderArmy.Units.FirstOrDefault(u => u.IsAlive)
                : defenderArmy.Units.LastOrDefault(u => u.IsAlive);
        }

        public List<(int, int)> GetMeleePairs(IArmy attackerArmy, IArmy defenderArmy, bool attackerIsArmy1)
        {
            var attacker = GetMeleeAttacker(attackerArmy, attackerIsArmy1);
            var defender = GetMeleeDefender(defenderArmy, attackerIsArmy1);
            if (attacker == null || defender == null) return new();
            int aIdx = attackerArmy.Units.ToList().IndexOf(attacker);
            int dIdx = defenderArmy.Units.ToList().IndexOf(defender);
            return new() { (aIdx, dIdx) };
        }

        public int GetDistanceBetweenUnits(IArmy myArmy, int myIndex, IArmy enemyArmy, int enemyIndex, bool isArmy1)
        {
            int myToFront = CountAliveBetween(myArmy, myIndex, GetFrontIndex(myArmy, isArmy1), isArmy1);
            int enemyFront = GetFrontIndex(enemyArmy, !isArmy1);
            int enemyToFront = CountAliveBetween(enemyArmy, enemyIndex, enemyFront, !isArmy1);
            return myToFront + 1 + enemyToFront + 1;
        }

        public int GetDistanceBetweenAllies(IArmy army, int index1, int index2, bool isArmy1)
        {
            int front = GetFrontIndex(army, isArmy1);
            int d1 = CountAliveBetween(army, index1, front, isArmy1);
            int d2 = CountAliveBetween(army, index2, front, isArmy1);
            return Math.Abs(d1 - d2);
        }

        private int GetFrontIndex(IArmy army, bool isArmy1)
        {
            if (isArmy1)
            {
                for (int i = army.Units.Count - 1; i >= 0; i--)
                    if (army.Units[i].IsAlive) return i;
            }
            else
            {
                for (int j = 0; j < army.Units.Count; j++)
                    if (army.Units[j].IsAlive) return j;
            }
            return -1;
        }

        private int CountAliveBetween(IArmy army, int unitIndex, int frontIndex, bool isArmy1)
        {
            int count = 0;
            if (isArmy1)
            {
                for (int i = unitIndex + 1; i <= frontIndex; i++)
                    if (army.Units[i].IsAlive) count++;
            }
            else
            {
                for (int j = frontIndex; j < unitIndex; j++)
                    if (army.Units[j].IsAlive) count++;
            }
            return count;
        }
    }
}