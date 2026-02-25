using System;
using System.Linq;
using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly IRandomService _random;

        private int _scoreArmy1 = 0;
        private int _scoreArmy2 = 0;

        private const int DistanceBetweenArmies = 1;

        public BattleField(
            IDamageCalculator damageCalculator,
            IBattleLogger logger,
            IRandomService random)
        {
            _damageCalculator = damageCalculator;
            _random = random;
        }

        public BattleResult StartBattle(IArmy army1, IArmy army2)
        {
            int turns = 0;

            bool army1Turn = _random.Next(0, 2) == 0;

            WriteArmyName(
                $"Первой атакует: {(army1Turn ? army1.Name : army2.Name)}",
                army1Turn);

            Console.ReadLine();

            while (HasAlive(army1) && HasAlive(army2))
            {
                BattleVisualizer.PrintArmyLine(army1, army2);
                Console.WriteLine();

                if (army1Turn)
                {
                    // A удар
                    Melee(army1, army2, true);
                    Wait();

                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // B ответ
                    Melee(army2, army1, false);
                    Wait();

                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // A лучники
                    ArcherPhase(army1, army2, true);
                    Wait();

                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // B лучники
                    ArcherPhase(army2, army1, false);
                    Wait();
                }
                else
                {
                    // B удар
                    Melee(army2, army1, false);
                    Wait();

                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // A ответ
                    Melee(army1, army2, true);
                    Wait();

                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // B лучники
                    ArcherPhase(army2, army1, false);
                    Wait();

                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // A лучники
                    ArcherPhase(army1, army2, true);
                    Wait();
                }

                Console.WriteLine($"\nСЧЁТ: {_scoreArmy1} : {_scoreArmy2}");
                Console.WriteLine("Нажмите Enter для следующего хода...");
                Console.ReadLine();

                // ===== УДАЛЕНИЕ ПОГИБШИХ ПОСЛЕ ПОЛНОГО РАУНДА =====
                army1.RemoveDeadUnits();
                army2.RemoveDeadUnits();

                turns++;
            }

            string winner = HasAlive(army1) ? army1.Name : army2.Name;

            return new BattleResult(winner, turns);
        }

        // ================== БЛИЖНИЙ БОЙ ==================

        private void Melee(IArmy attackerArmy, IArmy defenderArmy, bool attackerIsArmy1)
        {
            var attacker = attackerIsArmy1
                ? GetFrontArmy1(attackerArmy)
                : GetFrontArmy2(attackerArmy);

            var defender = attackerIsArmy1
                ? GetFrontArmy2(defenderArmy)
                : GetFrontArmy1(defenderArmy);

            if (attacker == null || defender == null)
                return;

            int damage = _damageCalculator.CalculateDamage(attacker, defender);
            int oldHp = defender.Health;

            defender.TakeDamage(damage);

            WriteUnit(attacker.Name, attackerIsArmy1);
            Console.Write(" атакует ");
            WriteUnit(defender.Name, !attackerIsArmy1);
            Console.WriteLine($" и наносит {damage} урона");

            Console.WriteLine(
                $"   {attacker.Name} -> HP: {attacker.Health}, DEF: {attacker.Defence}");
            Console.WriteLine(
                $"   {defender.Name} -> HP: {oldHp} -> {defender.Health}");

            if (!defender.IsAlive)
            {
                AddScore(attackerIsArmy1, defender.Cost);
                WriteUnit(defender.Name, !attackerIsArmy1);
                Console.WriteLine(" погиб!");
            }
        }

        // ================== ЛУЧНИКИ ==================

        private void ArcherPhase(IArmy army, IArmy enemy, bool isArmy1)
        {
            if (!HasAlive(army) || !HasAlive(enemy))
                return;

            bool anyArcher = false;

            int frontIndex = isArmy1
                ? GetFrontIndexArmy1(army)
                : GetFrontIndexArmy2(army);

            if (frontIndex == -1)
                return;

            if (isArmy1)
            {
                for (int i = frontIndex - 1; i >= 0; i--)
                {
                    if (army.Units[i].IsAlive && army.Units[i] is Archer archer)
                    {
                        anyArcher = true;
                        ProcessShot(army, enemy, archer, i, true);
                    }
                }
            }
            else
            {
                for (int i = frontIndex + 1; i < army.Units.Count; i++)
                {
                    if (army.Units[i].IsAlive && army.Units[i] is Archer archer)
                    {
                        anyArcher = true;
                        ProcessShot(army, enemy, archer, i, false);
                    }
                }
            }

            if (!anyArcher)
                Console.WriteLine($"В армии {army.Name} лучников нет.");
        }

        private void ProcessShot(
            IArmy army,
            IArmy enemy,
            Archer archer,
            int archerIndex,
            bool isArmy1)
        {
            int frontIndex = isArmy1
                ? GetFrontIndexArmy1(army)
                : GetFrontIndexArmy2(army);

            int aliveBetween = 0;

            if (isArmy1)
            {
                for (int i = archerIndex + 1; i <= frontIndex; i++)
                    if (army.Units[i].IsAlive)
                        aliveBetween++;
            }
            else
            {
                for (int i = frontIndex; i < archerIndex; i++)
                    if (army.Units[i].IsAlive)
                        aliveBetween++;
            }

            int distance = aliveBetween + DistanceBetweenArmies;

            WriteUnit(archer.Name, isArmy1);
            Console.WriteLine(
                $" стреляет на {archer.Range}, дистанция до врага {distance}");

            if (archer.Range < distance)
            {
                Console.WriteLine("Стрела не долетает.");
                return;
            }

            var target = isArmy1
                ? GetFrontArmy2(enemy)
                : GetFrontArmy1(enemy);

            if (target == null)
                return;

            int oldHp = target.Health;
            int damage = Math.Max(0, archer.Attack - target.Defence);

            target.TakeDamage(damage);

            WriteUnit(archer.Name, isArmy1);
            Console.Write(" попадает в ");
            WriteUnit(target.Name, !isArmy1);
            Console.WriteLine($" | HP: {oldHp} -> {target.Health}");

            if (!target.IsAlive)
            {
                AddScore(isArmy1, target.Cost);
                WriteUnit(target.Name, !isArmy1);
                Console.WriteLine(" погиб!");
            }
        }

        // ================== ФРОНТЫ ==================

        private IUnit GetFrontArmy1(IArmy army)
        {
            for (int i = army.Units.Count - 1; i >= 0; i--)
                if (army.Units[i].IsAlive)
                    return army.Units[i];
            return null;
        }

        private IUnit GetFrontArmy2(IArmy army)
        {
            for (int i = 0; i < army.Units.Count; i++)
                if (army.Units[i].IsAlive)
                    return army.Units[i];
            return null;
        }

        private int GetFrontIndexArmy1(IArmy army)
        {
            for (int i = army.Units.Count - 1; i >= 0; i--)
                if (army.Units[i].IsAlive)
                    return i;
            return -1;
        }

        private int GetFrontIndexArmy2(IArmy army)
        {
            for (int i = 0; i < army.Units.Count; i++)
                if (army.Units[i].IsAlive)
                    return i;
            return -1;
        }

        // ================== ВСПОМОГАТЕЛЬНЫЕ ==================

        private bool HasAlive(IArmy army)
            => army.Units.Any(u => u.IsAlive);

        private void AddScore(bool army1, int cost)
        {
            if (army1) _scoreArmy1 += cost;
            else _scoreArmy2 += cost;
        }

        private void WriteUnit(string name, bool isArmy1)
        {
            Console.ForegroundColor =
                isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write(name);
            Console.ResetColor();
        }

        private void WriteArmyName(string text, bool isArmy1)
        {
            Console.ForegroundColor =
                isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private void Wait()
        {
            Console.WriteLine("Нажмите Enter...");
            Console.ReadLine();
        }
    }
}