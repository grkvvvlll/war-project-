using System;
using gaaameee.Core.Interfaces;

namespace Services.Logging
{
    public class ConsoleBattleLogger : IBattleLogger
    {
        public void Log(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public void LogInfo(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public void LogSpecial(
            IUnit user,
            IUnit target,
            string abilityName,
            int damage)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{user.Name} ");

            Console.ResetColor();
            Console.Write($"использует способность '{abilityName}' на ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(target.Name);

            Console.ResetColor();
            Console.WriteLine($" и наносит {damage} урона");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $"   {user.Name} -> HP: {user.Health}, DEF: {user.Defence}");
            Console.WriteLine(
                $"   {target.Name} -> HP: {target.Health}, DEF: {target.Defence}");

            Console.ResetColor();
        }

        // ===== БЛИЖНИЙ БОЙ =====

        public void LogHit(
            IUnit attacker,
            IUnit defender,
            int damage,
            int oldHp,
            bool attackerIsArmy1)
        {
            Console.ForegroundColor =
                attackerIsArmy1 ? ConsoleColor.White : ConsoleColor.Red;

            Console.Write(attacker.Name);

            Console.ResetColor();
            Console.Write(" атакует ");

            Console.ForegroundColor =
                attackerIsArmy1 ? ConsoleColor.Red : ConsoleColor.White;

            Console.Write(defender.Name);

            Console.ResetColor();
            Console.WriteLine($" и наносит {damage} урона");

            Console.WriteLine(
                $"   {defender.Name} -> HP: {oldHp} -> {defender.Health}");
        }

        public void LogDeath(IUnit unit, bool isArmy1)
        {
            Console.ForegroundColor =
                isArmy1 ? ConsoleColor.White : ConsoleColor.Red;

            Console.WriteLine($"{unit.Name} погиб!");
            Console.ResetColor();
        }

        // ===== ЛУЧНИКИ =====

        public void LogArcherShot(
            IUnit archer,
            int range,
            int distance,
            bool isArmy1)
        {
            Console.ForegroundColor =
                isArmy1 ? ConsoleColor.White : ConsoleColor.Red;

            Console.WriteLine(
                $"{archer.Name} стреляет на {range}, дистанция до врага {distance}");

            Console.ResetColor();
        }

        public void LogArrowMiss()
        {
            Console.WriteLine("Стрела не долетает.");
        }

        public void LogArcherHit(
            IUnit archer,
            IUnit target,
            int oldHp,
            int newHp,
            bool isArmy1)
        {
            Console.ForegroundColor =
                isArmy1 ? ConsoleColor.White : ConsoleColor.Red;

            Console.Write(archer.Name);

            Console.ResetColor();
            Console.Write(" попадает в ");

            Console.ForegroundColor =
                isArmy1 ? ConsoleColor.Red : ConsoleColor.White;

            Console.Write(target.Name);

            Console.ResetColor();
            Console.WriteLine($" | HP: {oldHp} -> {newHp}");
        }

        public void LogNoArchers(string armyName)
        {
            Console.WriteLine(
                $"В армии {armyName} лучников нет.");
        }
    }
}