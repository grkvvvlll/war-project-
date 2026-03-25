using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Entities.Units;
using Core.Entities.Abilities;

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

            // ✅ Отслеживаем юниты, которые уже отыграли
            var processedUnits = new HashSet<IUnit>();

            for (int i = 0; i < army.Units.Count; i++)
            {
                var unit = army.Units[i];

                if (!unit.IsAlive || unit.SpecialAbility == null)
                    continue;

                // ✅ Пропускаем, если юнит уже отыграл
                if (processedUnits.Contains(unit))
                    continue;

                if (!unit.SpecialAbility.CanUse(unit))
                    continue;

                if (i == frontIndex)
                    continue;

                // === 🔮 ОСОБАЯ ЛОГИКА ДЛЯ МАГА ===
                if (unit is Wizard wizard)
                {
                    var (wizardTarget, wizardDistance) = FindAllyTargetForWizard(wizard, army, isArmy1, i);

                    int currentChance = ((CloneAbility)unit.SpecialAbility).GetCurrentChance();
                    Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                    Console.Write("Вероятность клонирования юнита магом - ");
                    Console.ResetColor();
                    Console.WriteLine($"{currentChance}%.");

                    if (wizardTarget == null)
                    {
                        unit.SpecialAbility.Charge();
                        int newChance = ((CloneAbility)unit.SpecialAbility).GetCurrentChance();
                        Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                        Console.Write($"{unit.Name} ");
                        Console.ResetColor();
                        Console.WriteLine($"никого не клонировал в этом раунде. Вероятность клонирования выросла до {newChance}%");
                        processedUnits.Add(unit);
                        continue;
                    }

                    bool isAlly = army.Units.Contains(wizardTarget);
                    if (!unit.SpecialAbility.CanTarget(unit, wizardTarget, isAlly))
                    {
                        unit.SpecialAbility.Charge();
                        int newChance = ((CloneAbility)unit.SpecialAbility).GetCurrentChance();
                        Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                        Console.Write($"{unit.Name} ");
                        Console.ResetColor();
                        Console.WriteLine($"никого не клонировал в этом раунде. Вероятность клонирования выросла до {newChance}%");
                        processedUnits.Add(unit);
                        continue;
                    }

                    if (unit.SpecialAbility is CloneAbility cloneAbility)
                    {
                        int wizardIndex = i;
                        string targetName = wizardTarget.Name;  

                        Action<IUnit, IUnit>? handler = null;
                        handler = (user, clone) =>
                        {
                            int insertPosition = isArmy1 ? wizardIndex + 1 : wizardIndex;
                            army.InsertUnit(clone, insertPosition);
                            processedUnits.Add(clone);

                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"✨ {user.Name} ");
                            Console.ResetColor();
                            // ✅ Используем targetName (оригинал), а не clone.Name (клон)
                            Console.WriteLine($"склонировал {targetName} и поставил перед собой.");
                        };
                        cloneAbility.CloneCreated += handler;
                        unit.SpecialAbility.Use(unit, wizardTarget, wizardDistance);
                        cloneAbility.CloneCreated -= handler;
                    }
                    else
                    {
                        unit.SpecialAbility.Use(unit, wizardTarget, wizardDistance);
                    }

                    int chanceAfter = ((CloneAbility)unit.SpecialAbility).GetCurrentChance();
                    if (chanceAfter > wizard.ClonePower)
                    {
                        Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                        Console.Write($"{unit.Name} ");
                        Console.ResetColor();
                        Console.WriteLine($"никого не клонировал в этом раунде.Вероятность клонирования выросла до {chanceAfter}%");
                    }

                    processedUnits.Add(unit);
                    continue;
                }

                // === 🏹 ЛУЧНИКИ и ЦЕЛИТЕЛИ ===
                IUnit? target = FindTarget(unit, army, enemy, isArmy1);

                if (target == null)
                {
                    unit.SpecialAbility.Charge();
                    continue;
                }

                bool targetIsAlly = army.Units.Contains(target);
                if (!unit.SpecialAbility.CanTarget(unit, target, targetIsAlly))
                    continue;

                // ✅ ОБЪЯВЛЯЕМ ПЕРЕМЕННЫЕ ОДИН РАЗ ЗДЕСЬ
                int distance = CalculateDistance(army, i, frontIndex, isArmy1);
                int oldHp = target.Health;

                unit.SpecialAbility.Use(unit, target, distance);

                if (unit is Archer archer)
                {
                    _logger.LogArcherShot(archer, archer.Range, distance, isArmy1);

                    if (archer.Range < distance)
                    {
                        _logger.LogArrowMiss();
                    }
                    else if (target.Health < oldHp)
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

                processedUnits.Add(unit);
            }

            return totalScore;
        }

        // === 🔮 Поиск СОЮЗНИКА для Мага в радиусе ===
        // === 🔮 Поиск СОЮЗНИКА для Мага + дистанция до него ===
        private (IUnit? target, int distance) FindAllyTargetForWizard(Wizard wizard, IArmy army, bool isArmy1, int wizardIndex)
        {
            var armyUnitsList = army.Units.ToList();
            int wizardFrontIndex = GetFrontIndex(army, isArmy1);
            int wizardToOwnFront = CountAliveBetween(army, wizardIndex, wizardFrontIndex, isArmy1);

            var validTargets = new List<(IUnit unit, int distance)>();

            foreach (var ally in armyUnitsList.Where(u => u.IsAlive && u != wizard))
            {
                if (!(ally is LightUnit || ally is Archer))
                    continue;

                int allyIndex = armyUnitsList.IndexOf(ally);
                int allyToOwnFront = CountAliveBetween(army, allyIndex, wizardFrontIndex, isArmy1);

                // ✅ Расстояние между магом и союзником
                int distance = Math.Abs(wizardToOwnFront - allyToOwnFront);

                if (distance <= wizard.SpellRange)
                {
                    validTargets.Add((ally, distance));
                }
            }

            if (validTargets.Any())
            {
                var chosen = validTargets[_random.Next(validTargets.Count)];
                return (chosen.unit, chosen.distance);  // ✅ Возвращаем кортеж
            }

            return (null, -1);
        }

        // === 🏹 Поиск цели для Лучника и Целителя ===
        private IUnit? FindTarget(IUnit user, IArmy army, IArmy enemy, bool isArmy1)
        {
            if (user is Archer archer)
            {
                var validTargets = new List<(IUnit unit, int distance)>();
                int enemyFrontIndex = GetFrontIndex(enemy, !isArmy1);
                int archerFrontIndex = GetFrontIndex(army, isArmy1);
                int archerIndex = army.Units.ToList().IndexOf(user);
                int archerToOwnFront = CountAliveBetween(army, archerIndex, archerFrontIndex, isArmy1);

                foreach (var enemyUnit in enemy.Units.Where(u => u.IsAlive))
                {
                    int enemyIndex = enemy.Units.ToList().IndexOf(enemyUnit);
                    int enemyToOwnFront = CountAliveBetween(enemy, enemyIndex, enemyFrontIndex, !isArmy1);
                    int distance = archerToOwnFront + 1 + enemyToOwnFront;

                    if (distance <= archer.Range)
                        validTargets.Add((enemyUnit, distance));
                }

                if (validTargets.Any())
                    return validTargets[_random.Next(validTargets.Count)].unit;

                return isArmy1 ? enemy.Units.FirstOrDefault(u => u.IsAlive) : enemy.Units.LastOrDefault(u => u.IsAlive);
            }

            if (user is Healer healer)
            {
                var armyUnitsList = army.Units.ToList();
                int healerIndex = armyUnitsList.IndexOf(user);
                int healerFrontIndex = GetFrontIndex(army, isArmy1);
                int healerToOwnFront = CountAliveBetween(army, healerIndex, healerFrontIndex, isArmy1);

                var validTargets = new List<IUnit>();

                foreach (var ally in armyUnitsList.Where(u => u.IsAlive && u != user))
                {
                    // ✅ ИСКЛЮЧАЕМ HeavyUnit — их нельзя лечить
                    if (ally is HeavyUnit)
                        continue;

                    int allyIndex = armyUnitsList.IndexOf(ally);
                    int allyToOwnFront = CountAliveBetween(army, allyIndex, healerFrontIndex, isArmy1);
                    int distance = Math.Abs(healerToOwnFront - allyToOwnFront);

                    if (distance <= healer.HealRange)
                    {
                        validTargets.Add(ally);
                    }
                }

                if (validTargets.Any())
                {
                    return validTargets[_random.Next(validTargets.Count)];
                }

                return null;
            }

            return isArmy1 ? enemy.Units.FirstOrDefault(u => u.IsAlive) : enemy.Units.LastOrDefault(u => u.IsAlive);
        }

        private int CalculateDistance(IArmy army, int unitIndex, int frontIndex, bool isArmy1)
        {
            return CountAliveBetween(army, unitIndex, frontIndex, isArmy1) + DistanceBetweenArmies;
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
                for (int i = 0; i < army.Units.Count; i++)
                    if (army.Units[i].IsAlive) return i;
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
                for (int i = frontIndex; i < unitIndex; i++)
                    if (army.Units[i].IsAlive) count++;
            }
            return count;
        }
    }
}