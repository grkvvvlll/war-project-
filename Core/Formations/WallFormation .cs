using Core.Interfaces;

namespace Core.Formations
{
    public class WallFormation : IBattleFormation
    {
        private const int GapBetweenArmies = 1;

        public string Name => "Стенка на стенку";
        public string Description => "Одна колонна, юниты выстроены сверху вниз. Бьются попарно по строкам.";

        // ── Позиция юнита: строка = позиция в списке живых, столбец = 0 ────────
        public (int row, int col) GetPosition(IArmy army, int unitIndex, bool isArmy1)
        {
            var map = GetAlivePositionMap(army);
            if (!map.TryGetValue(unitIndex, out var pos)) return (0, 0);
            return pos;
        }

        // ── Все юниты на фронте ────────────────────────────────────────────────
        public bool IsOnFrontLine(IArmy army, int unitIndex, bool isArmy1)
        {
            return true; // все юниты всегда на фронте
        }

        // ── Special ability только если напротив пусто ────────────────────────
        public bool CanUseSpecialAbility(IArmy myArmy, int unitIndex, IArmy enemyArmy, bool isArmy1)
        {
            var myMap = GetAlivePositionMap(myArmy);
            var enemyMap = GetAlivePositionMap(enemyArmy);

            if (!myMap.TryGetValue(unitIndex, out var myPos)) return false;

            // Проверяем есть ли враг в той же строке
            bool hasOpponent = enemyMap.Values.Any(p => p.row == myPos.row);
            return !hasOpponent;
        }

        // ── Ближний бой: пары по строкам ──────────────────────────────────────
        public List<(int, int)> GetMeleePairs(IArmy attackerArmy, IArmy defenderArmy, bool attackerIsArmy1)
        {
            var pairs = new List<(int, int)>();

            var attackerMap = GetAlivePositionMap(attackerArmy);
            var defenderMap = GetAlivePositionMap(defenderArmy);

            // row → unitIndex для каждой армии
            var attackerByRow = attackerMap.ToDictionary(kv => kv.Value.row, kv => kv.Key);
            var defenderByRow = defenderMap.ToDictionary(kv => kv.Value.row, kv => kv.Key);

            foreach (var (row, aIdx) in attackerByRow)
            {
                if (defenderByRow.TryGetValue(row, out int dIdx))
                    pairs.Add((aIdx, dIdx));
            }

            return pairs;
        }

        public IUnit? GetMeleeAttacker(IArmy attackerArmy, bool attackerIsArmy1)
        {
            return attackerArmy.Units.FirstOrDefault(u => u.IsAlive);
        }

        public IUnit? GetMeleeDefender(IArmy defenderArmy, bool attackerIsArmy1)
        {
            return defenderArmy.Units.FirstOrDefault(u => u.IsAlive);
        }

        // ── Дистанция до врага: разница строк + зазор ─────────────────────────
        public int GetDistanceBetweenUnits(IArmy myArmy, int myIndex, IArmy enemyArmy, int enemyIndex, bool isArmy1)
        {
            var myMap = GetAlivePositionMap(myArmy);
            var enMap = GetAlivePositionMap(enemyArmy);

            if (!myMap.TryGetValue(myIndex, out var myPos)) return 999;
            if (!enMap.TryGetValue(enemyIndex, out var enPos)) return 999;

            return Math.Abs(myPos.row - enPos.row) + GapBetweenArmies;
        }

        // ── Дистанция до союзника: разница строк ──────────────────────────────
        public int GetDistanceBetweenAllies(IArmy army, int index1, int index2, bool isArmy1)
        {
            var map = GetAlivePositionMap(army);

            if (!map.TryGetValue(index1, out var p1)) return 999;
            if (!map.TryGetValue(index2, out var p2)) return 999;

            return Math.Abs(p1.row - p2.row);
        }

        // ── Карта позиций: unitIndex → (row, col) ─────────────────────────────
        // Заполнение сверху вниз по живым юнитам в порядке их индекса
        public Dictionary<int, (int row, int col)> GetAlivePositionMap(IArmy army)
        {
            var result = new Dictionary<int, (int, int)>();
            int row = 0;

            for (int i = 0; i < army.Units.Count; i++)
            {
                if (!army.Units[i].IsAlive) continue;
                result[i] = (row, 0);
                row++;
            }

            return result;
        }
    }
}
