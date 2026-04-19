using Core.Entities.Abilities;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Interfaces;
using Core.Entities;
using Core.Formations;

namespace Services.Battle
{
    public class SpecialAbilityService
    {
        private readonly IBattleLogger _logger;
        private readonly System.Random _random;
        private IBattleFormation _formation;

        public SpecialAbilityService(IBattleLogger logger, IBattleFormation formation)
        {
            _logger = logger;
            _random = new System.Random();
            _formation = formation;
        }

        public void SetFormation(IBattleFormation formation) => _formation = formation;

        public int Execute(IArmy army, IArmy enemy, bool isArmy1)
        {
            if (!army.Units.Any(u => u.IsAlive) || !enemy.Units.Any(u => u.IsAlive))
                return 0;

            int totalScore = 0;
            var processedUnits = new HashSet<IUnit>();

            for (int i = 0; i < army.Units.Count; i++)
            {
                var unit = army.Units[i];

                if (!unit.IsAlive || unit.SpecialAbility == null || processedUnits.Contains(unit))
                    continue;

                if (!unit.SpecialAbility.CanUse(unit))
                    continue;

                if (!_formation.CanUseSpecialAbility(army, i, enemy, isArmy1))
                {
                    processedUnits.Add(unit);
                    continue;
                }

                // === ОРУЖЕНОСЕЦ (LightUnit) ===
                if (unit is LightUnit)
                {
                    HandleSquire(unit, army, i, isArmy1, processedUnits);
                    processedUnits.Add(unit);
                    continue;
                }

                // === МАГ ===
                if (unit is Wizard wizard)
                {
                    HandleWizard(wizard, unit, army, i, isArmy1, processedUnits);
                    processedUnits.Add(unit);
                    continue;
                }

                // === ЛУЧНИК и ЦЕЛИТЕЛЬ ===
                IUnit? target = FindTarget(unit, army, enemy, i, isArmy1);

                if (target == null)
                {
                    unit.SpecialAbility.Charge();
                    processedUnits.Add(unit);
                    continue;
                }

                bool targetIsAlly = army.Units.Contains(target);

                if (!unit.SpecialAbility.CanTarget(unit, target, targetIsAlly))
                {
                    processedUnits.Add(unit);
                    continue;
                }

                int targetIndex = (targetIsAlly ? army : enemy).Units.ToList().IndexOf(target);
                int distance = targetIsAlly
                    ? GetAllyDistance(army, i, targetIndex, isArmy1)
                    : GetEnemyDistance(army, i, enemy, targetIndex, isArmy1);

                int oldHp = target.Health;
                unit.SpecialAbility.Use(unit, target, distance);

                if (unit is Archer archer)
                {
                    _logger.LogArcherShot(archer, archer.Range, distance, isArmy1);
                    if (archer.Range < distance)
                        _logger.LogArrowMiss();
                    else
                        _logger.LogArcherHit(unit, target, oldHp, target.Health, isArmy1);
                }
                else if (unit is Healer)
                {
                    int healed = target.Health - oldHp;
                    if (healed > 0)
                    {
                        Console.Write("💚 ");
                        _logger.LogHeal(unit, target, healed, isArmy1);
                    }
                    else
                        _logger.LogHealNoEffect(unit, target, isArmy1);
                }
                else
                {
                    _logger.LogSpecial(unit, target, unit.SpecialAbility.Name, Math.Abs(oldHp - target.Health));
                }

                if (!target.IsAlive && oldHp > 0)
                {
                    _logger.LogDeath(target, !isArmy1);
                    totalScore += target.Cost;
                }

                processedUnits.Add(unit);
            }

            return totalScore;
        }

        // ── Дистанция до врага ────────────────────────────────────────────────
        private int GetEnemyDistance(IArmy myArmy, int myIndex, IArmy enemyArmy, int enemyIndex, bool isArmy1)
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

            // BridgeFormation — старая логика
            int myFront = GetBridgeFrontIndex(myArmy, isArmy1);
            int enFront = GetBridgeFrontIndex(enemyArmy, !isArmy1);
            int myToFront = CountBridgeAliveBetween(myArmy, myIndex, myFront, isArmy1);
            int enToFront = CountBridgeAliveBetween(enemyArmy, enemyIndex, enFront, !isArmy1);
            return myToFront + 1 + enToFront + 1;
        }

        // ── Дистанция до союзника ─────────────────────────────────────────────
        private int GetAllyDistance(IArmy army, int index1, int index2, bool isArmy1)
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

            // BridgeFormation — старая линейная логика
            int front = GetBridgeFrontIndex(army, isArmy1);
            int d1 = CountBridgeAliveBetween(army, index1, front, isArmy1);
            int d2 = CountBridgeAliveBetween(army, index2, front, isArmy1);
            return Math.Abs(d1 - d2);
        }

        // ── Оруженосец ────────────────────────────────────────────────────────
        private void HandleSquire(IUnit unit, IArmy army, int i, bool isArmy1, HashSet<IUnit> processedUnits)
        {
            if (unit.SpecialAbility is not SquireAbility squireAbility) return;

            List<int> neighborIndices;
            if (_formation is WideBridgeFormation)
                neighborIndices = GetNeighborIndicesWide(army, i, isArmy1, maxDist: 1);
            else if (_formation is WallFormation wall)
                neighborIndices = GetNeighborIndicesByRow(army, i, maxDist: 1, wall);
            else
            {
                neighborIndices = new List<int>();
                if (i > 0) neighborIndices.Add(i - 1);
                if (i < army.Units.Count - 1) neighborIndices.Add(i + 1);
            }

            IUnit? targetHeavy = null;
            int targetIndex = -1;

            foreach (var idx in neighborIndices)
            {
                var neighbor = army.Units[idx];
                if (!neighbor.IsAlive) continue;
                if (!unit.SpecialAbility.CanTarget(unit, neighbor, true)) continue;
                if (GetBuffCount(neighbor) >= 4) continue;

                targetHeavy = neighbor;
                targetIndex = idx;
                break;
            }

            if (targetHeavy == null) return;

            string targetBaseName = targetHeavy.Name;
            int oldAttack = targetHeavy.Attack;
            int oldDefence = targetHeavy.Defence;

            squireAbility.Use(unit, targetHeavy, 0);

            if (squireAbility.LastAppliedUnit == null) return;

            ((Army)army).SetUnit(targetIndex, squireAbility.LastAppliedUnit);

            string buffName = "Бафф";
            int atkDelta = 0, defDelta = 0;

            if (squireAbility.LastAppliedUnit is UnitDecorator dec)
            {
                var buff = dec.GetCurrentBuff();
                buffName = buff.NameNominative;
                atkDelta = buff.AttackBonus;
                defDelta = buff.DefenceBonus;
            }

            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write($"{unit.Name} ");
            Console.ResetColor();
            Console.Write("добавил ");
            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write($"{targetBaseName} ");
            Console.ResetColor();
            Console.WriteLine($"бафф: {buffName}");

            if (atkDelta != 0 || defDelta != 0)
            {
                Console.Write("   Характеристики: ");
                if (atkDelta != 0) Console.Write($"ATK {oldAttack} -> {oldAttack + atkDelta}");
                if (atkDelta != 0 && defDelta != 0) Console.Write(", ");
                if (defDelta != 0) Console.Write($"DEF {oldDefence} -> {oldDefence + defDelta}");
                Console.WriteLine();
            }
        }

        // ── Маг ───────────────────────────────────────────────────────────────
        private void HandleWizard(Wizard wizard, IUnit unit, IArmy army, int i, bool isArmy1, HashSet<IUnit> processedUnits)
        {
            if (unit.SpecialAbility is not CloneAbility cloneAbility) return;

            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write("Вероятность клонирования юнита магом - ");
            Console.ResetColor();
            Console.WriteLine($"{cloneAbility.GetCurrentChance()}%.");

            var candidates = new List<(IUnit unit, int dist)>();
            for (int j = 0; j < army.Units.Count; j++)
            {
                var ally = army.Units[j];
                if (!ally.IsAlive || ally == unit) continue;
                if (!(ally is LightUnit || ally is Archer)) continue;

                int dist = GetAllyDistance(army, i, j, isArmy1);
                if (dist <= wizard.SpellRange)
                    candidates.Add((ally, dist));
            }

            if (!candidates.Any())
            {
                unit.SpecialAbility.Charge();
                Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                Console.Write($"{unit.Name} ");
                Console.ResetColor();
                Console.WriteLine($"никого не клонировал. Вероятность выросла до {cloneAbility.GetCurrentChance()}%");
                return;
            }

            var (wizardTarget, wizardDist) = candidates[_random.Next(candidates.Count)];

            if (!unit.SpecialAbility.CanTarget(unit, wizardTarget, true))
            {
                unit.SpecialAbility.Charge();
                Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                Console.Write($"{unit.Name} ");
                Console.ResetColor();
                Console.WriteLine($"никого не клонировал. Вероятность выросла до {cloneAbility.GetCurrentChance()}%");
                return;
            }

            string targetName = wizardTarget.Name;

            Action<IUnit, IUnit>? handler = null;
            handler = (user, clone) =>
            {
                int insertPosition;
                if (_formation is WideBridgeFormation)
                    insertPosition = i;
                else
                    insertPosition = isArmy1 ? i + 1 : i;

                army.InsertUnit(clone, insertPosition);
                processedUnits.Add(clone);
                Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                Console.Write($"✨ {user.Name} ");
                Console.ResetColor();
                Console.WriteLine($"склонировал {targetName}.");
            };

            cloneAbility.CloneCreated += handler;
            unit.SpecialAbility.Use(unit, wizardTarget, wizardDist);
            cloneAbility.CloneCreated -= handler;

            if (cloneAbility.GetCurrentChance() > wizard.ClonePower)
            {
                Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                Console.Write($"{unit.Name} ");
                Console.ResetColor();
                Console.WriteLine($"никого не клонировал. Вероятность выросла до {cloneAbility.GetCurrentChance()}%");
            }
        }

        // ── Найти цель для лучника / целителя ─────────────────────────────────
        private IUnit? FindTarget(IUnit user, IArmy army, IArmy enemy, int unitIndex, bool isArmy1)
        {
            if (user is Archer archer)
            {
                var validTargets = new List<(IUnit unit, int dist)>();

                if (_formation is WideBridgeFormation wideBridge)
                {
                    var myMap = wideBridge.GetAlivePositionMap(army, isArmy1);
                    var enMap = wideBridge.GetAlivePositionMap(enemy, !isArmy1);

                    if (!myMap.TryGetValue(unitIndex, out var myPos)) return null;

                    foreach (var (enIdx, enPos) in enMap)
                    {
                        int dist = myPos.col + enPos.col + 2;
                        if (dist <= archer.Range)
                            validTargets.Add((enemy.Units[enIdx], dist));
                    }

                    if (validTargets.Any())
                        return validTargets[_random.Next(validTargets.Count)].unit;

                    return enMap
                        .OrderBy(kv => kv.Value.col)
                        .Select(kv => enemy.Units[kv.Key])
                        .FirstOrDefault();
                }
                else if (_formation is WallFormation wall)
                {
                    // Стенка: стреляет вверх/вниз по столбцу врага
                    for (int j = 0; j < enemy.Units.Count; j++)
                    {
                        if (!enemy.Units[j].IsAlive) continue;
                        int dist = wall.GetDistanceBetweenUnits(army, unitIndex, enemy, j, isArmy1);
                        if (dist <= archer.Range)
                            validTargets.Add((enemy.Units[j], dist));
                    }

                    if (validTargets.Any())
                        return validTargets[_random.Next(validTargets.Count)].unit;

                    return enemy.Units
                        .Where(u => u.IsAlive)
                        .OrderBy(u => {
                            int j = enemy.Units.ToList().IndexOf(u);
                            return wall.GetDistanceBetweenUnits(army, unitIndex, enemy, j, isArmy1);
                        })
                        .FirstOrDefault();
                }
                else
                {
                    // BridgeFormation — старая логика
                    int enemyFrontIndex = GetBridgeFrontIndex(enemy, !isArmy1);
                    int archerFrontIndex = GetBridgeFrontIndex(army, isArmy1);
                    int archerToOwnFront = CountBridgeAliveBetween(army, unitIndex, archerFrontIndex, isArmy1);

                    foreach (var enemyUnit in enemy.Units.Where(u => u.IsAlive))
                    {
                        int enIdx = enemy.Units.ToList().IndexOf(enemyUnit);
                        int enemyToOwnFront = CountBridgeAliveBetween(enemy, enIdx, enemyFrontIndex, !isArmy1);
                        int distance = archerToOwnFront + 1 + enemyToOwnFront + 1;

                        if (distance <= archer.Range)
                            validTargets.Add((enemyUnit, distance));
                    }

                    if (validTargets.Any())
                        return validTargets[_random.Next(validTargets.Count)].unit;

                    return isArmy1
                        ? enemy.Units.FirstOrDefault(u => u.IsAlive)
                        : enemy.Units.LastOrDefault(u => u.IsAlive);
                }
            }

            if (user is Healer healer)
            {
                var validTargets = new List<IUnit>();

                if (_formation is WideBridgeFormation wideBridge)
                {
                    var map = wideBridge.GetAlivePositionMap(army, isArmy1);
                    if (!map.TryGetValue(unitIndex, out var myPos)) return null;

                    for (int j = 0; j < army.Units.Count; j++)
                    {
                        var ally = army.Units[j];
                        if (!ally.IsAlive || ally == user) continue;
                        if (ally is HeavyUnit) continue;
                        if (!map.TryGetValue(j, out var allyPos)) continue;

                        int dist = Math.Max(Math.Abs(myPos.row - allyPos.row), Math.Abs(myPos.col - allyPos.col));
                        if (dist <= healer.HealRange)
                            validTargets.Add(ally);
                    }
                }
                else if (_formation is WallFormation wall)
                {
                    // Стенка: лечит вверх/вниз по своему столбцу
                    for (int j = 0; j < army.Units.Count; j++)
                    {
                        var ally = army.Units[j];
                        if (!ally.IsAlive || ally == user) continue;
                        if (ally is HeavyUnit) continue;

                        int dist = wall.GetDistanceBetweenAllies(army, unitIndex, j, isArmy1);
                        if (dist <= healer.HealRange)
                            validTargets.Add(ally);
                    }
                }
                else
                {
                    // BridgeFormation — старая логика
                    int healerFrontIndex = GetBridgeFrontIndex(army, isArmy1);
                    int healerToOwnFront = CountBridgeAliveBetween(army, unitIndex, healerFrontIndex, isArmy1);

                    foreach (var ally in army.Units.Where(u => u.IsAlive && u != user))
                    {
                        if (ally is HeavyUnit) continue;
                        int allyIdx = army.Units.ToList().IndexOf(ally);
                        int allyToOwnFront = CountBridgeAliveBetween(army, allyIdx, healerFrontIndex, isArmy1);
                        int dist = Math.Abs(healerToOwnFront - allyToOwnFront);
                        if (dist <= healer.HealRange)
                            validTargets.Add(ally);
                    }
                }

                if (validTargets.Any())
                    return validTargets[_random.Next(validTargets.Count)];
                return null;
            }

            return null;
        }

        // ── Соседние индексы для широкого моста (чебышёв ≤ maxDist) ──────────
        private List<int> GetNeighborIndicesWide(IArmy army, int unitIndex, bool isArmy1, int maxDist)
        {
            var result = new List<int>();
            if (_formation is not WideBridgeFormation wideBridge) return result;

            var map = wideBridge.GetAlivePositionMap(army, isArmy1);
            if (!map.TryGetValue(unitIndex, out var myPos)) return result;

            for (int j = 0; j < army.Units.Count; j++)
            {
                if (j == unitIndex || !army.Units[j].IsAlive) continue;
                if (!map.TryGetValue(j, out var pos)) continue;
                int dist = Math.Max(Math.Abs(myPos.row - pos.row), Math.Abs(myPos.col - pos.col));
                if (dist <= maxDist)
                    result.Add(j);
            }
            return result;
        }

        // ── Соседние индексы для стенки (только по строкам ≤ maxDist) ────────
        private List<int> GetNeighborIndicesByRow(IArmy army, int unitIndex, int maxDist, WallFormation wall)
        {
            var result = new List<int>();
            var map = wall.GetAlivePositionMap(army);
            if (!map.TryGetValue(unitIndex, out var myPos)) return result;

            for (int j = 0; j < army.Units.Count; j++)
            {
                if (j == unitIndex || !army.Units[j].IsAlive) continue;
                if (!map.TryGetValue(j, out var pos)) continue;
                int dist = Math.Abs(myPos.row - pos.row);
                if (dist <= maxDist)
                    result.Add(j);
            }
            return result;
        }

        // ── Вспомогательные методы для BridgeFormation ────────────────────────
        private int GetBridgeFrontIndex(IArmy army, bool isArmy1)
        {
            if (isArmy1)
            {
                for (int i = army.Units.Count - 1; i >= 0; i--)
                    if (army.Units[i].IsAlive) return i;
            }
            else
            {
                for (int i = 0; i < army.Units.Count; i++)
                    if (army.Units[i].IsAlive) return i;
            }
            return -1;
        }

        private int CountBridgeAliveBetween(IArmy army, int unitIndex, int frontIndex, bool isArmy1)
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

        private int GetBuffCount(IUnit unit)
        {
            int count = 0;
            IUnit current = unit;
            while (current is UnitDecorator decorator)
            {
                count++;
                current = decorator.GetInnerUnit();
            }
            return count;
        }
    }
}