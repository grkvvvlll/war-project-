using Core.Interfaces;

namespace Core.Formations
{
    public class WideBridgeFormation : IBattleFormation
    {
        private const int Rows = 3;
        private const int GapBetweenArmies = 1;

        public string Name => "Бой на широком мосту";
        public string Description => "3 строки юнитов. Фронт бьётся попарно по строкам. Юниты за фронтом используют способности.";

        public (int row, int col) GetPosition(IArmy army, int unitIndex, bool isArmy1)
        {
            var alive = GetAlivePositionMap(army, isArmy1);
            if (!alive.TryGetValue(unitIndex, out var pos)) return (0, 0);
            return pos;
        }

        public bool IsOnFrontLine(IArmy army, int unitIndex, bool isArmy1)
        {
            var (_, col) = GetPosition(army, unitIndex, isArmy1);
            return col == 0;
        }

        public bool CanUseSpecialAbility(IArmy myArmy, int unitIndex, IArmy enemyArmy, bool isArmy1)
        {
            var (myRow, myCol) = GetPosition(myArmy, unitIndex, isArmy1);

            if (myCol > 0) return true;

            // На фронте: проверяем, есть ли враг напротив в той же строке
            var enemyMap = GetAlivePositionMap(enemyArmy, !isArmy1);
            bool hasOpponent = enemyMap.Values.Any(p => p.row == myRow && p.col == 0);
            return !hasOpponent;
        }

        public List<(int, int)> GetMeleePairs(IArmy attackerArmy, IArmy defenderArmy, bool attackerIsArmy1)
        {
            var pairs = new List<(int, int)>();

            var attackerMap = GetAlivePositionMap(attackerArmy, attackerIsArmy1);
            var defenderMap = GetAlivePositionMap(defenderArmy, !attackerIsArmy1);

            var attackerFront = attackerMap
                .Where(kv => kv.Value.col == 0)
                .ToDictionary(kv => kv.Value.row, kv => kv.Key);

            var defenderFront = defenderMap
                .Where(kv => kv.Value.col == 0)
                .ToDictionary(kv => kv.Value.row, kv => kv.Key);

            foreach (var (row, aIdx) in attackerFront)
            {
                if (defenderFront.TryGetValue(row, out int dIdx))
                {
                    pairs.Add((aIdx, dIdx));
                }
            }

            return pairs;
        }

        public IUnit? GetMeleeAttacker(IArmy attackerArmy, bool attackerIsArmy1)
        {
            var map = GetAlivePositionMap(attackerArmy, attackerIsArmy1);
            var front = map.Where(kv => kv.Value.col == 0 && kv.Value.row == 0);
            if (!front.Any()) return null;
            return attackerArmy.Units[front.First().Key];
        }

        public IUnit? GetMeleeDefender(IArmy defenderArmy, bool attackerIsArmy1)
        {
            var map = GetAlivePositionMap(defenderArmy, !attackerIsArmy1);
            var front = map.Where(kv => kv.Value.col == 0 && kv.Value.row == 0);
            if (!front.Any()) return null;
            return defenderArmy.Units[front.First().Key];
        }

        public int GetDistanceBetweenUnits(IArmy myArmy, int myIndex, IArmy enemyArmy, int enemyIndex, bool isArmy1)
        {
            var (myRow, myCol) = GetPosition(myArmy, myIndex, isArmy1);
            var (enRow, enCol) = GetPosition(enemyArmy, enemyIndex, !isArmy1);

            int rowDiff = Math.Abs(myRow - enRow);
            int colDiff = myCol + GapBetweenArmies + enCol;

            return Math.Max(rowDiff, colDiff);
        }

        public int GetDistanceBetweenAllies(IArmy army, int index1, int index2, bool isArmy1)
        {
            var (r1, c1) = GetPosition(army, index1, isArmy1);
            var (r2, c2) = GetPosition(army, index2, isArmy1);
            return Math.Max(Math.Abs(r1 - r2), Math.Abs(c1 - c2));
        }

        public Dictionary<int, (int row, int col)> GetAlivePositionMap(IArmy army, bool isArmy1 = true)
        {
            var result = new Dictionary<int, (int, int)>();

            var aliveIndices = isArmy1
                ? Enumerable.Range(0, army.Units.Count)
                    .Where(i => army.Units[i].IsAlive)
                    .Reverse()
                    .ToList()
                : Enumerable.Range(0, army.Units.Count)
                    .Where(i => army.Units[i].IsAlive)
                    .ToList();

            for (int slot = 0; slot < aliveIndices.Count; slot++)
            {
                int unitIndex = aliveIndices[slot];
                int col = slot / Rows;
                int row = slot % Rows;
                result[unitIndex] = (row, col);
            }

            return result;
        }
    }
}