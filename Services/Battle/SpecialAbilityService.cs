using System;
using System.Linq;
using Core.Interfaces;
using Core.Entities.Units;

namespace Services.Battle
{
    public class SpecialAbilityService
    {
        private readonly System.Random _random;
        private readonly IBattleLogger _logger;
        private const int DistanceBetweenArmies = 1;

        public SpecialAbilityService(IBattleLogger logger)
        {
            _logger = logger;
            _random = new System.Random();
        }

        public int Execute(IArmy army, IArmy enemy, bool isArmy1)
        {
            if (!army.Units.Any(u => u.IsAlive) ||
                !enemy.Units.Any(u => u.IsAlive))
                return 0;

            int totalScore = 0;
            int frontIndex = GetFrontIndex(army, isArmy1);

            if (frontIndex == -1)
                return 0;

            for (int i = 0; i < army.Units.Count; i++)
            {
                var unit = army.Units[i];

                if (!unit.IsAlive || unit.SpecialAbility == null)
                    continue;

                if (!unit.SpecialAbility.CanUse(unit))
                    continue;

                // Юниты на передней линии не используют способности
                if (i == frontIndex)
                    continue;

                IUnit target = FindTarget(unit, army, enemy, isArmy1);
                if (target == null)
                {
                    unit.SpecialAbility.Charge();
                    continue;
                }

                bool isAlly = army.Units.Contains(target);

                if (!unit.SpecialAbility.CanTarget(unit, target, isAlly))
                    continue;

                int distance = CalculateDistance(army, i, frontIndex, isArmy1);

                if (unit is Archer archer)
                {
                    _logger.LogArcherShot(archer, archer.Range, distance, isArmy1);

                    if (archer.Range < distance)
                    {
                        _logger.LogArrowMiss();
                        continue;
                    }
                }

                int oldHp = target.Health;

                unit.SpecialAbility.Use(unit, target, distance);

                if (unit is Archer)
                {
                    if (target.Health < oldHp)
                    {
                        _logger.LogArcherHit(unit, target, oldHp, target.Health, isArmy1);
                    }
                }
                else if (unit is Healer)
                {
                    int healed = target.Health - oldHp;
                    if (healed > 0)
                    {
                        _logger.LogHeal(unit, target, healed, isArmy1);
                    }
                    else
                    {
                        _logger.LogHealNoEffect(unit, target, isArmy1);
                    }
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
            }

            return totalScore;
        }

        private IUnit? FindTarget(IUnit user, IArmy army, IArmy enemy, bool isArmy1)
        {
            if (user is Archer archer)
            {
                var validTargets = new List<(IUnit unit, int distance)>();
                int enemyFrontIndex = GetFrontIndex(enemy, !isArmy1);
                int archerFrontIndex = GetFrontIndex(army, isArmy1);
                int archerIndex = army.Units.ToList().IndexOf(user);

                // Считаем, сколько живых юнитов между лучником и фронтом СВОЕЙ армии
                int archerToOwnFront = CountAliveBetween(army, archerIndex, archerFrontIndex, isArmy1);

                foreach (var enemyUnit in enemy.Units.Where(u => u.IsAlive))
                {
                    int enemyIndex = enemy.Units.ToList().IndexOf(enemyUnit);
                    int enemyToOwnFront = CountAliveBetween(enemy, enemyIndex, enemyFrontIndex, !isArmy1);

                    // Полная дистанция = (лучник → свой фронт) + (между армиями) + (вражеский фронт → цель)
                    int distance = archerToOwnFront + 1 + enemyToOwnFront;

                    if (distance <= archer.Range)
                    {
                        validTargets.Add((enemyUnit, distance));
                    }
                }

                if (validTargets.Any())
                {
                    // Выбираем случайную цель из доступных
                    var random = new System.Random();
                    return validTargets[random.Next(validTargets.Count)].unit;
                }

                // Если нет целей в диапазоне — стреляем в первого живого
                return isArmy1
                    ? enemy.Units.FirstOrDefault(u => u.IsAlive)
                    : enemy.Units.LastOrDefault(u => u.IsAlive);
            }

            // === ДЛЯ ЦЕЛИТЕЛЕЙ: ищем самого раненого союзника ===
            if (user is Healer healer)
            {
                var armyUnitsList = army.Units.ToList();
                int healerIndex = armyUnitsList.IndexOf(user);
                int healerFrontIndex = GetFrontIndex(army, isArmy1);

                // Считаем позицию целителя относительно фронта
                int healerToOwnFront = CountAliveBetween(army, healerIndex, healerFrontIndex, isArmy1);

                // Находим всех союзников в радиусе (healer.Range)
                var validTargets = new List<IUnit>();

                foreach (var ally in armyUnitsList.Where(u => u.IsAlive && u != user && !(u is HeavyUnit)))
                {
                    int allyIndex = armyUnitsList.IndexOf(ally);
                    int allyToOwnFront = CountAliveBetween(army, allyIndex, healerFrontIndex, isArmy1);

                    // Дистанция между целителем и союзником
                    int distance = Math.Abs(healerToOwnFront - allyToOwnFront);

                    if (distance <= healer.HealRange)
                    {
                        validTargets.Add(ally);
                    }
                }

                if (validTargets.Any())
                {
                    // Выбираем СЛУЧАЙНОГО союзника
                    return validTargets[_random.Next(validTargets.Count)];
                }

                return null;
            }

                // === ДЛЯ ОСТАЛЬНЫХ: первый живой враг ===
                return isArmy1
                ? enemy.Units.FirstOrDefault(u => u.IsAlive)
                : enemy.Units.LastOrDefault(u => u.IsAlive);
        }

        private int CalculateDistance(IArmy army, int unitIndex, int frontIndex, bool isArmy1)
        {
            int aliveBetween = CountAliveBetween(army, unitIndex, frontIndex, isArmy1);
            return aliveBetween + DistanceBetweenArmies;
        }

        private int GetFrontIndex(IArmy army, bool isArmy1)
        {
            if (isArmy1)
            {
                for (int i = army.Units.Count - 1; i >= 0; i--)
                {
                    if (army.Units[i].IsAlive)
                        return i;
                }
            }
            else
            {
                for (int i = 0; i < army.Units.Count; i++)
                {
                    if (army.Units[i].IsAlive)
                        return i;
                }
            }
            return -1;
        }

        private int CountAliveBetween(IArmy army, int unitIndex, int frontIndex, bool isArmy1)
        {
            int count = 0;

            if (isArmy1)
            {
                for (int i = unitIndex + 1; i <= frontIndex; i++)
                {
                    if (army.Units[i].IsAlive)
                        count++;
                }
            }
            else
            {
                for (int i = frontIndex; i < unitIndex; i++)
                {
                    if (army.Units[i].IsAlive)
                        count++;
                }
            }

            return count;
        }
    }
}