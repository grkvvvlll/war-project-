using Core.Entities.Buffs;
using Core.Interfaces;

namespace WpfPresentation.Engine
{
    public class WpfBattleLogger : IBattleLogger
    {
        public List<BattleEvent> Events { get; } = new();

        private IArmy? _army1;
        private IArmy? _army2;

        public void SetArmies(IArmy army1, IArmy army2)
        {
            _army1 = army1;
            _army2 = army2;
        }

        public void Log(string message) { }
        public void LogInfo(string message) { }
        public void LogNoArchers(string armyName) { }
        public void LogArrowMiss() { }
        public void Clear() => Events.Clear();

        public void LogHit(IUnit attacker, IUnit defender, int damage, int oldHp, bool attackerIsArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = damage > 0 ? BattleEventType.MeleeHit : BattleEventType.MeleeMiss,
                ActorIsArmy1 = attackerIsArmy1,
                ActorIndex = FindIndex(attacker, attackerIsArmy1),
                ActorName = attacker.Name,
                TargetIsArmy1 = !attackerIsArmy1,
                TargetIndex = FindIndex(defender, !attackerIsArmy1),
                TargetName = defender.Name,
                Damage = damage,
                HpBefore = oldHp,
                HpAfter = defender.Health
            });
        }

        public void LogDeath(IUnit unit, bool isArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = BattleEventType.Death,
                TargetIsArmy1 = isArmy1,
                TargetIndex = FindIndex(unit, isArmy1),
                TargetName = unit.Name
            });
        }

        public void LogArcherShot(IUnit archer, int range, int distance, bool isArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = distance <= range ? BattleEventType.ArrowShot : BattleEventType.ArrowMiss,
                ActorIsArmy1 = isArmy1,
                ActorIndex = FindIndex(archer, isArmy1),
                ActorName = archer.Name,
                Damage = distance
            });
        }

        public void LogArcherHit(IUnit archer, IUnit target, int oldHp, int newHp, bool isArmy1)
        {
            // Находим ранее добавленное ArrowShot-событие и дополняем его данными о цели
            for (int i = Events.Count - 1; i >= 0; i--)
            {
                if (Events[i].Type == BattleEventType.ArrowShot && Events[i].ActorName == archer.Name)
                {
                    Events[i] = Events[i] with
                    {
                        TargetIsArmy1 = !isArmy1,
                        TargetIndex = FindIndex(target, !isArmy1),
                        TargetName = target.Name,
                        HpBefore = oldHp,
                        HpAfter = newHp,
                        Damage = oldHp - newHp
                    };
                    break;
                }
            }
        }

        public void LogHeal(IUnit healer, IUnit target, int healedAmount, bool healerIsArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = BattleEventType.Heal,
                ActorIsArmy1 = healerIsArmy1,
                ActorIndex = FindIndex(healer, healerIsArmy1),
                ActorName = healer.Name,
                TargetIsArmy1 = healerIsArmy1,   // целитель лечит союзника
                TargetIndex = FindIndex(target, healerIsArmy1),
                TargetName = target.Name,
                Damage = healedAmount,
                HpBefore = target.Health - healedAmount,
                HpAfter = target.Health
            });
        }

        public void LogHealNoEffect(IUnit healer, IUnit target, bool healerIsArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = BattleEventType.HealNoEffect,
                ActorIsArmy1 = healerIsArmy1,
                ActorIndex = FindIndex(healer, healerIsArmy1),
                ActorName = healer.Name,
                TargetIsArmy1 = healerIsArmy1,
                TargetIndex = FindIndex(target, healerIsArmy1),
                TargetName = target.Name
            });
        }

        public void LogSpecial(IUnit user, IUnit target, string abilityName, int damage)
        {
            int idxInArmy1 = FindInArmy(user, isArmy1: true);
            bool actorIsArmy1 = idxInArmy1 >= 0;
            int actorIndex = actorIsArmy1 ? idxInArmy1 : FindIndex(user, isArmy1: false);

            // Цель может быть союзником (клонирует) или врагом
            int targetIdxSameArmy = FindInArmy(target, actorIsArmy1);
            bool targetIsArmy1 = targetIdxSameArmy >= 0 ? actorIsArmy1 : !actorIsArmy1;
            int targetIndex = FindIndex(target, targetIsArmy1);

            Events.Add(new BattleEvent
            {
                Type = BattleEventType.Spell,
                ActorIsArmy1 = actorIsArmy1,
                ActorIndex = actorIndex,
                ActorName = user.Name,
                TargetIsArmy1 = targetIsArmy1,
                TargetIndex = targetIndex,
                TargetName = target.Name,
                Message = abilityName,
                Damage = damage
            });
        }

        // Вспомогательные методы поиска 

        // Возвращает индекс юнита в армии
        private int FindIndex(IUnit unit, bool isArmy1)
        {
            int idx = FindInArmy(unit, isArmy1);
            return idx >= 0 ? idx : 0;
        }

        /// <summary>
        /// Ищет юнита в армии сначала по ссылке на объект,
        /// затем по базовому имени
        /// </summary>
        private int FindInArmy(IUnit unit, bool isArmy1)
        {
            var army = isArmy1 ? _army1 : _army2;
            if (army == null) return -1;

            var list = army.Units.ToList();

            // 1. По ссылке
            int idx = list.IndexOf(unit);
            if (idx >= 0) return idx;

            // 2. По базовому имени (разворачиваем декораторы с обеих сторон)
            string baseName = GetBaseName(unit);
            for (int i = 0; i < list.Count; i++)
                if (GetBaseName(list[i]) == baseName) return i;

            return -1;
        }

        /// <summary>
        /// Разворачивает цепочку UnitDecorator и возвращает имя базового юнита.
        /// </summary>
        private static string GetBaseName(IUnit unit)
        {
            IUnit current = unit;
            while (current is UnitDecorator dec)
                current = dec.GetInnerUnit();
            return current.Name;
        }

        public void LogBuffAdded(IUnit squire, IUnit target, string buffName, bool isArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = BattleEventType.BuffAdded,
                ActorIsArmy1 = isArmy1,
                ActorIndex = FindIndex(squire, isArmy1),
                ActorName = squire.Name,
                TargetIsArmy1 = isArmy1,
                TargetIndex = FindIndex(target, isArmy1),
                TargetName = target.Name,
                Message = buffName
            });
        }

        // Потеря баффа в WPF не требует отдельного события 
        public void LogBuffLost(IUnit unit, string buffName, bool attackerIsArmy1) { }

        public void LogCloneChance(IUnit wizard, int chancePercent, bool isArmy1) { }
        public void LogCloneFailed(IUnit wizard, int newChancePercent, bool isArmy1) { }
        public void LogCloneSuccess(IUnit wizard, string targetName, bool isArmy1) { }
    }
}