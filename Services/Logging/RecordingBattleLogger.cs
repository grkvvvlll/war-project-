using System;
using System.Collections.Generic;
using Core.Interfaces;

namespace Services.Logging
{
    public class RecordingBattleLogger : IRecordingBattleLogger
    {
        private readonly IBattleLogger _inner;

        public List<string> Lines { get; } = new();

        public RecordingBattleLogger(IBattleLogger inner)
        {
            _inner = inner;
        }

        public void Clear() => Lines.Clear();

        private void Add(string line) => Lines.Add(line);

        public void Log(string message)
        {
            _inner.Log(message);
            Add(message);
        }

        public void LogInfo(string message)
        {
            _inner.LogInfo(message);
            Add(message);
        }

        public void LogSpecial(IUnit user, IUnit target, string abilityName, int damage)
        {
            _inner.LogSpecial(user, target, abilityName, damage);

            Add($"{user.Name} использует способность '{abilityName}' на {target.Name} и наносит {damage} урона");
            Add($"   {user.Name} -> HP: {user.Health}, DEF: {user.Defence}");
            Add($"   {target.Name} -> HP: {target.Health}, DEF: {target.Defence}");
        }

        public void LogHit(IUnit attacker, IUnit defender, int damage, int oldHp, bool attackerIsArmy1)
        {
            _inner.LogHit(attacker, defender, damage, oldHp, attackerIsArmy1);

            Add($"{attacker.Name} атакует {defender.Name} и наносит {damage} урона");
            Add($"   {defender.Name} -> HP: {oldHp} -> {defender.Health}");
        }

        public void LogDeath(IUnit unit, bool isArmy1)
        {
            _inner.LogDeath(unit, isArmy1);
            Add($"{unit.Name} погиб!");
        }

        public void LogArcherShot(IUnit archer, int range, int distance, bool isArmy1)
        {
            _inner.LogArcherShot(archer, range, distance, isArmy1);
            Add($"{archer.Name} стреляет на {range}, дистанция до врага {distance}");
        }

        public void LogHeal(IUnit healer, IUnit target, int healedAmount, bool healerIsArmy1)
        {
            _inner.LogHeal(healer, target, healedAmount, healerIsArmy1);
            Add($"{healer.Name} лечит {target.Name} и восстанавливает {healedAmount} HP");
            Add($"   {target.Name} -> HP: {target.Health - healedAmount} -> {target.Health}");
        }

        public void LogHealNoEffect(IUnit healer, IUnit target, bool healerIsArmy1)
        {
            _inner.LogHealNoEffect(healer, target, healerIsArmy1);
            Add($"{healer.Name} выбирает {target.Name}, HP максимальное, юнит в лечении не нуждается");
        }

        public void LogArrowMiss()
        {
            _inner.LogArrowMiss();
            Add("Стрела не долетает.");
        }

        public void LogArcherHit(IUnit archer, IUnit target, int oldHp, int newHp, bool isArmy1)
        {
            _inner.LogArcherHit(archer, target, oldHp, newHp, isArmy1);
            Add($"{archer.Name} попадает в {target.Name} | HP: {oldHp} -> {newHp}");
        }

        public void LogNoArchers(string armyName)
        {
            _inner.LogNoArchers(armyName);
            Add($"В армии {armyName} лучников нет.");
        }
    }
}