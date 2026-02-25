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

        public void LogHit(IUnit attacker, IUnit defender, int damage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{attacker.Name} ");

            Console.ResetColor();
            Console.Write("атакует ");

            // Цвет для защитника
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{defender.Name}");

            Console.ResetColor();
            Console.WriteLine($" и наносит {damage} урона");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   {attacker.Name} -> HP: {attacker.Health}, DEF: {attacker.Defence}");
            Console.WriteLine($"   {defender.Name} -> HP: {defender.Health}, DEF: {defender.Defence}");
            Console.ResetColor();
        }

        // Лог использования спецспособности.
        public void LogSpecial(IUnit user, IUnit target, string abilityName, int damage)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{user.Name} ");

            Console.ResetColor();
            Console.Write($"использует способность '{abilityName}' на ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(target.Name);

            Console.ResetColor();
            Console.WriteLine($" и наносит {damage} урона");

            // HP после удара
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   {user.Name} -> HP: {user.Health}, DEF: {user.Defence}");
            Console.WriteLine($"   {target.Name} -> HP: {target.Health}, DEF: {target.Defence}");
            Console.ResetColor();
        }

        /// Лог смерти юнита.
        public void LogDeath(IUnit unit)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{unit.Name} погиб!");
            Console.ResetColor();
        }

        public void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}