using System;
using System.Threading;
using Core.Interfaces;

namespace Services.Logging
{
    public class ConsoleBattleLogger : IBattleLogger
    {
        private void SlowWrite(string text)
        {
            Console.WriteLine(text);
            Thread.Sleep(30);
        }

        public void Log(string message) 
        {
            Console.ResetColor();
            SlowWrite(message);
        }

        public void LogInfo(string message)
        {
            Console.ResetColor();
            SlowWrite(message);
        }

        public void LogHeal(IUnit healer, IUnit target, int healedAmount, bool healerIsArmy1)
        {
            Console.ForegroundColor = healerIsArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write($"{healer.Name} ");
            Console.ResetColor();
            Console.Write("лечит ");

            Console.ForegroundColor = healerIsArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write(target.Name);
            Console.ResetColor();
            SlowWrite($" и восстанавливает {healedAmount} HP");

            SlowWrite($"   {target.Name} -> HP: {target.Health - healedAmount} -> {target.Health}");
        }

        public void LogHealNoEffect(IUnit healer, IUnit target, bool healerIsArmy1)
        {
            Console.ForegroundColor = healerIsArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write($"{healer.Name} ");
            Console.ResetColor();
            Console.Write("выбирает ");
            Console.ForegroundColor = healerIsArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write(target.Name);
            Console.ResetColor();
            SlowWrite($", HP максимальное, юнит в лечении не нуждается");
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
            SlowWrite($" и наносит {damage} урона");

            Console.ForegroundColor = ConsoleColor.Yellow;
            SlowWrite($"   {user.Name} -> HP: {user.Health}, DEF: {user.Defence}");
            SlowWrite($"   {target.Name} -> HP: {target.Health}, DEF: {target.Defence}");

            Console.ResetColor();
        }

        public void LogHit(
            IUnit attacker,
            IUnit defender,
            int damage,
            int oldHp,
            bool attackerIsArmy1)
        {
            Console.ForegroundColor = attackerIsArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write(attacker.Name);

            Console.ResetColor();
            Console.Write(" атакует ");

            Console.ForegroundColor = attackerIsArmy1 ? ConsoleColor.Red : ConsoleColor.White;
            Console.Write(defender.Name);

            Console.ResetColor();
            SlowWrite($" и наносит {damage} урона");

            SlowWrite($"   {defender.Name} -> HP: {oldHp} -> {defender.Health}");
        }

        public void LogDeath(IUnit unit, bool isArmy1)
        {
            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            SlowWrite($"{unit.Name} погиб!");
            Console.ResetColor();
        }

        public void LogArcherShot(IUnit archer, int range, int distance, bool isArmy1)
        {
            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            SlowWrite($"{archer.Name} стреляет на {range}, дистанция до врага {distance}");
            Console.ResetColor();
        }

        public void LogArrowMiss()
        {
            SlowWrite("Стрела не долетает.");
        }

        public void LogArcherHit(IUnit archer, IUnit target, int oldHp, int newHp, bool isArmy1)
        {
            Console.ForegroundColor = isArmy1 ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write(archer.Name);

            Console.ResetColor();
            Console.Write(" попадает в ");

            Console.ForegroundColor = isArmy1 ? ConsoleColor.Red : ConsoleColor.White;
            Console.Write(target.Name);

            Console.ResetColor();
            SlowWrite($" | HP: {oldHp} -> {newHp}");
        }

        public void LogNoArchers(string armyName)
        {
            SlowWrite($"В армии {armyName} лучников нет.");
        }
    }
}