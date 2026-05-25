using Core.Entities.Buffs;
using Core.Formations;
using Core.Interfaces;
using Services.Observers;

namespace Services.Battle
{
    /// <summary>
    /// Выполняет специальные способности юнитов.
    /// Реализует IAbilityExecutionContext, предоставляя способностям вспомогательные методы
    /// без прямой зависимости на сервис — устранение нарушения OCP (type-checks по типу юнита).
    /// </summary>
    public class SpecialAbilityService : ISpecialAbilityService, IAbilityExecutionContext
    {
        private readonly IBattleLogger _logger;
        private readonly IRandomService _random;
        private IBattleFormation _formation;
        private HashSet<IUnit> _processedInCurrentRound = new();

        public IBattleFormation Formation => _formation;
        public IBattleLogger Logger => _logger;
        public IRandomService Random => _random;

        public SpecialAbilityService(IBattleLogger logger, IRandomService random, IBattleFormation formation)
        {
            _logger = logger;
            _random = random;
            _formation = formation;
        }

        public void SetFormation(IBattleFormation formation) => _formation = formation;

        // ── Главный метод — без единой проверки is LightUnit/Wizard/Archer/Healer ──
        public int Execute(IArmy army, IArmy enemy, bool isArmy1)
        {
            if (!army.Units.Any(u => u.IsAlive) || !enemy.Units.Any(u => u.IsAlive))
                return 0;

            _processedInCurrentRound = new HashSet<IUnit>();
            int totalScore = 0;

            for (int i = 0; i < army.Units.Count; i++)
            {
                var unit = army.Units[i];

                if (!unit.IsAlive || unit.SpecialAbility == null) continue;
                if (_processedInCurrentRound.Contains(unit)) continue;
                if (!unit.SpecialAbility.CanUse(unit)) continue;
                if (!_formation.CanUseSpecialAbility(army, i, enemy, isArmy1))
                {
                    _processedInCurrentRound.Add(unit);
                    continue;
                }

                totalScore += unit.SpecialAbility.Execute(unit, i, army, enemy, isArmy1, this);
                _processedInCurrentRound.Add(unit);
            }

            _processedInCurrentRound = new HashSet<IUnit>();
            return totalScore;
        }

        // ── IAbilityExecutionContext — вспомогательные методы ─────────────────

        public void RegisterNewUnit(IUnit unit)
        {
            ObserverRegistry.Instance.Attach(unit);
            _processedInCurrentRound.Add(unit);
        }

        public int GetEnemyDistance(IArmy myArmy, int myIndex,
                                    IArmy enemyArmy, int enemyIndex, bool isArmy1)
        {
            if (_formation is WideBridgeFormation wideBridge)
            {
                var myMap = wideBridge.GetAlivePositionMap(myArmy, isArmy1);
                var enMap = wideBridge.GetAlivePositionMap(enemyArmy, !isArmy1);
                if (!myMap.TryGetValue(myIndex, out var myPos)) return 999;
                if (!enMap.TryGetValue(enemyIndex, out var enPos)) return 999;
                return myPos.col + enPos.col + 2;
            }

            if (_formation is WallFormation wall)
                return wall.GetDistanceBetweenUnits(myArmy, myIndex, enemyArmy, enemyIndex, isArmy1);

            int myFront = GetBridgeFrontIndex(myArmy, isArmy1);
            int enFront = GetBridgeFrontIndex(enemyArmy, !isArmy1);
            int myToFront = CountBridgeAliveBetween(myArmy, myIndex, myFront, isArmy1);
            int enToFront = CountBridgeAliveBetween(enemyArmy, enemyIndex, enFront, !isArmy1);
            return myToFront + 1 + enToFront + 1;
        }

        public int GetAllyDistance(IArmy army, int index1, int index2, bool isArmy1)
        {
            if (_formation is WideBridgeFormation wideBridge)
            {
                var map = wideBridge.GetAlivePositionMap(army, isArmy1);
                if (!map.TryGetValue(index1, out var p1)) return 999;
                if (!map.TryGetValue(index2, out var p2)) return 999;
                return Math.Max(Math.Abs(p1.row - p2.row), Math.Abs(p1.col - p2.col));
            }

            if (_formation is WallFormation wall)
                return wall.GetDistanceBetweenAllies(army, index1, index2, isArmy1);

            int front = GetBridgeFrontIndex(army, isArmy1);
            int d1 = CountBridgeAliveBetween(army, index1, front, isArmy1);
            int d2 = CountBridgeAliveBetween(army, index2, front, isArmy1);
            return Math.Abs(d1 - d2);
        }

        public List<int> GetNeighborIndices(IArmy army, int unitIndex, bool isArmy1, int maxDist)
        {
            if (_formation is WideBridgeFormation wideBridge)
            {
                var result = new List<int>();
                var map = wideBridge.GetAlivePositionMap(army, isArmy1);
                if (!map.TryGetValue(unitIndex, out var myPos)) return result;
                for (int j = 0; j < army.Units.Count; j++)
                {
                    if (j == unitIndex || !army.Units[j].IsAlive) continue;
                    if (!map.TryGetValue(j, out var pos)) continue;
                    int dist = Math.Max(Math.Abs(myPos.row - pos.row), Math.Abs(myPos.col - pos.col));
                    if (dist <= maxDist) result.Add(j);
                }
                return result;
            }

            if (_formation is WallFormation wall)
            {
                var result = new List<int>();
                var map = wall.GetAlivePositionMap(army);
                if (!map.TryGetValue(unitIndex, out var myPos)) return result;
                for (int j = 0; j < army.Units.Count; j++)
                {
                    if (j == unitIndex || !army.Units[j].IsAlive) continue;
                    if (!map.TryGetValue(j, out var pos)) continue;
                    if (Math.Abs(myPos.row - pos.row) <= maxDist) result.Add(j);
                }
                return result;
            }

            // BridgeFormation — линейные соседи
            var linear = new List<int>();
            if (unitIndex > 0) linear.Add(unitIndex - 1);
            if (unitIndex < army.Units.Count - 1) linear.Add(unitIndex + 1);
            return linear;
        }

        // ── BridgeFormation helpers ────────────────────────────────────────────
        private int GetBridgeFrontIndex(IArmy army, bool isArmy1)
        {
            if (isArmy1)
                for (int i = army.Units.Count - 1; i >= 0; i--)
                    if (army.Units[i].IsAlive) return i;
                    else
                        for (int j = 0; j < army.Units.Count; j++)
                            if (army.Units[j].IsAlive) return j;
            return -1;
        }

        private int CountBridgeAliveBetween(IArmy army, int unitIndex, int frontIndex, bool isArmy1)
        {
            if (frontIndex < 0) return 0;

            int total = army.Units.Count;
            if (unitIndex < 0 || unitIndex >= total) return 0;

            // Clamp frontIndex в пределах списка на случай изменений состава армии
            frontIndex = Math.Min(frontIndex, total - 1);

            int count = 0;
            if (isArmy1)
            {
                for (int i = unitIndex + 1; i <= frontIndex; i++)
                    if (army.Units[i].IsAlive) count++;
            }
            else
            {
                for (int j = Math.Max(0, frontIndex); j < unitIndex; j++)
                    if (army.Units[j].IsAlive) count++;
            }
            return count;
        }
    }
}