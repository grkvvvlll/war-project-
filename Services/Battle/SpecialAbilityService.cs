using Core.Entities.Abilities;
using Core.Entities.Buffs; 
using Core.Entities.Units;
using Core.Interfaces;
using Core.Entities;

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

            // ✅ Отслеживаем юниты, которые уже отыграли, чтобы не использовать способность дважды за ход
            var processedUnits = new HashSet<IUnit>();

            // Проходим по всем юнитам армии
            for (int i = 0; i < army.Units.Count; i++)
            {
                var unit = army.Units[i];

                // Пропускаем мертвых, тех у кого нет способности или кто уже ходил
                if (!unit.IsAlive || unit.SpecialAbility == null || processedUnits.Contains(unit))
                    continue;

                if (!unit.SpecialAbility.CanUse(unit))
                    continue;

                // === 🛡️ ЛОГИКА ОРУЖЕНОСЦА (LIGHT UNIT) ===
                if (unit is LightUnit lightUnit)
                {
                    // Оруженосец не может баффать, если стоит на самой передней линии
                    if (i == frontIndex)
                    {
                        processedUnits.Add(unit);
                        continue;
                    }

                    // Ищем соседей (i-1 и i+1)
                    var neighborIndices = new List<int>();
                    if (i > 0) neighborIndices.Add(i - 1);
                    if (i < army.Units.Count - 1) neighborIndices.Add(i + 1);

                    IUnit? targetHeavy = null;
                    int targetIndex = -1;

                    foreach (var idx in neighborIndices)
                    {
                        var neighbor = army.Units[idx];

                        if (neighbor.IsAlive && unit.SpecialAbility.CanTarget(unit, neighbor, true))
                        {
                            int buffCount = GetBuffCount(neighbor);
                            if (buffCount < 4)
                            {
                                targetHeavy = neighbor;
                                targetIndex = idx;
                                break;
                            }
                        }
                    }

                    if (targetHeavy != null && unit.SpecialAbility is SquireAbility squireAbility)
                    {
                        // 1. ЗАПОМИНАЕМ состояние ДО применения баффа
                        string targetBaseName = targetHeavy.Name; // Имя без нового баффа (но со старыми, если есть)
                        int oldAttack = targetHeavy.Attack;
                        int oldDefence = targetHeavy.Defence;

                        // Пытаемся применить способность
                        squireAbility.Use(unit, targetHeavy, 0);

                        if (squireAbility.LastAppliedUnit != null)
                        {
                            // УСПЕХ: Бафф наложен

                            // 2. Заменяем юнита в армии
                            ((Army)army).SetUnit(targetIndex, squireAbility.LastAppliedUnit);

                            // 3. Получаем данные о новом баффе
                            string buffNameNominative = "Бафф";
                            int attackDelta = 0;
                            int defenceDelta = 0;

                            if (squireAbility.LastAppliedUnit is UnitDecorator dec)
                            {
                                var currentBuff = dec.GetCurrentBuff();
                                buffNameNominative = currentBuff.NameNominative; // "Копьё", "Щит" и т.д.
                                attackDelta = currentBuff.AttackBonus;
                                defenceDelta = currentBuff.DefenceBonus;
                            }

                            // 4. Формируем красивый лог
                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"{unit.Name} ");
                            Console.ResetColor();
                            Console.Write("добавил ");

                            // Используем имя цели ДО добавления текущего баффа, но ПОСЛЕ добавления предыдущих (если были)
                            // Так как targetHeavy.Name уже содержит старые баффы, а новый еще не "впаян" в имя переменной targetHeavy,
                            // но squireAbility.LastAppliedUnit.Name уже содержит ВСЕ баффы.
                            // Чтобы было красиво: "Light 2 добавил Heavy 3 (с Щитом) бафф: Копьё"
                            // Мы можем взять имя из LastAppliedUnit, но убрать самый последний бафф из строки? Сложно.
                            // Проще: вывести имя цели так, как оно было бы БЕЗ этого конкретного нового баффа.
                            // Но у нас нет простой ссылки на "предыдущее состояние".

                            // Хак: Мы знаем, что targetHeavy - это объект ДО замены. Его Name содержит старые баффы.
                            // А новый объект - это squireAbility.LastAppliedUnit.

                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"{targetBaseName} ");
                            Console.ResetColor();

                            Console.WriteLine($"бафф: {buffNameNominative}");

                            // 5. Выводим изменение характеристик
                            if (attackDelta != 0 || defenceDelta != 0)
                            {
                                Console.Write("   Характеристики: ");
                                if (attackDelta != 0)
                                {
                                    Console.Write($"ATK {oldAttack} -> {oldAttack + attackDelta}");
                                }
                                if (attackDelta != 0 && defenceDelta != 0)
                                {
                                    Console.Write(", ");
                                }
                                if (defenceDelta != 0)
                                {
                                    Console.Write($"DEF {oldDefence} -> {oldDefence + defenceDelta}");
                                }
                                Console.WriteLine();
                            }
                        }
                    }

                    processedUnits.Add(unit);
                    continue;
                }

                // === 🔮 ОСОБАЯ ЛОГИКА ДЛЯ МАГА ===
                if (unit is Wizard wizard)
                {
                    var (wizardTarget, wizardDistance) = FindAllyTargetForWizard(wizard, army, isArmy1, i);

                    // Приведение типа для доступа к шансу
                    if (unit.SpecialAbility is CloneAbility cloneAbilityCheck)
                    {
                        int currentChance = cloneAbilityCheck.GetCurrentChance();
                        Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                        Console.Write("Вероятность клонирования юнита магом - ");
                        Console.ResetColor();
                        Console.WriteLine($"{currentChance}%.");
                    }

                    if (wizardTarget == null)
                    {
                        unit.SpecialAbility.Charge();
                        if (unit.SpecialAbility is CloneAbility ca1)
                        {
                            int newChance = ca1.GetCurrentChance();
                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"{unit.Name} ");
                            Console.ResetColor();
                            Console.WriteLine($"никого не клонировал в этом раунде. Вероятность клонирования выросла до {newChance}%");
                        }
                        processedUnits.Add(unit);
                        continue;
                    }

                    bool isAlly = army.Units.Contains(wizardTarget);
                    if (!unit.SpecialAbility.CanTarget(unit, wizardTarget, isAlly))
                    {
                        unit.SpecialAbility.Charge();
                        if (unit.SpecialAbility is CloneAbility ca2)
                        {
                            int newChance = ca2.GetCurrentChance();
                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"{unit.Name} ");
                            Console.ResetColor();
                            Console.WriteLine($"никого не клонировал в этом раунде. Вероятность клонирования выросла до {newChance}%");
                        }
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
                            processedUnits.Add(clone); // Клон тоже считается обработанным, чтобы не стрелял в этот же ход

                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"✨ {user.Name} ");
                            Console.ResetColor();
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

                    if (unit.SpecialAbility is CloneAbility ca3)
                    {
                        int chanceAfter = ca3.GetCurrentChance();
                        if (chanceAfter > wizard.ClonePower) // Если шанс не сбросился, значит неудача
                        {
                            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
                            Console.Write($"{unit.Name} ");
                            Console.ResetColor();
                            Console.WriteLine($"никого не клонировал в этом раунде.Вероятность клонирования выросла до {chanceAfter}%");
                        }
                    }

                    processedUnits.Add(unit);
                    continue;
                }

                // === 🏹 ЛУЧНИКИ и ЦЕЛИТЕЛИ ===
                IUnit? target = FindTarget(unit, army, enemy, isArmy1);

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

                int distance = CalculateDistance(army, i, frontIndex, isArmy1);
                int oldHp = target.Health;

                unit.SpecialAbility.Use(unit, target, distance);

                // Логирование для конкретных классов
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
                    // Общая логика для остальных способностей
                    _logger.LogSpecial(unit, target, unit.SpecialAbility.Name, Math.Abs(oldHp - target.Health));
                }

                // Проверка смерти цели после способности
                if (!target.IsAlive && oldHp > 0)
                {
                    _logger.LogDeath(target, !isArmy1);
                    totalScore += target.Cost;
                }

                processedUnits.Add(unit);
            }

            return totalScore;
        }

        // === Вспомогательные методы ===

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

                // Расстояние между магом и союзником
                int distance = Math.Abs(wizardToOwnFront - allyToOwnFront);

                if (distance <= wizard.SpellRange)
                {
                    validTargets.Add((ally, distance));
                }
            }

            if (validTargets.Any())
            {
                var chosen = validTargets[_random.Next(validTargets.Count)];
                return (chosen.unit, chosen.distance);
            }

            return (null, -1);
        }

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
                    // ИСКЛЮЧАЕМ HeavyUnit — их нельзя лечить
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