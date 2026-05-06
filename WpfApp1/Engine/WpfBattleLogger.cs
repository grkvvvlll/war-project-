using Core.Interfaces;

namespace WpfPresentation.Engine
{
    public class WpfBattleLogger : IBattleLogger
    {
        public List<BattleEvent> Events { get; } = new();

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
                ActorName = attacker.Name,
                TargetIsArmy1 = !attackerIsArmy1,
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
                TargetName = unit.Name
            });
        }

        public void LogArcherShot(IUnit archer, int range, int distance, bool isArmy1)
        {
            Events.Add(new BattleEvent
            {
                Type = distance <= range ? BattleEventType.ArrowShot : BattleEventType.ArrowMiss,
                ActorIsArmy1 = isArmy1,
                ActorName = archer.Name,
                Damage = distance
            });
        }

        public void LogArcherHit(IUnit archer, IUnit target, int oldHp, int newHp, bool isArmy1)
        {
            // Находим последнее событие ArrowShot и обновляем его
            for (int i = Events.Count - 1; i >= 0; i--)
            {
                if (Events[i].Type == BattleEventType.ArrowShot && Events[i].ActorName == archer.Name)
                {
                    Events[i] = Events[i] with
                    {
                        TargetIsArmy1 = !isArmy1,
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
                ActorName = healer.Name,
                TargetIsArmy1 = healerIsArmy1,
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
                ActorName = healer.Name,
                TargetIsArmy1 = healerIsArmy1,
                TargetName = target.Name
            });
        }

        public void LogSpecial(IUnit user, IUnit target, string abilityName, int damage)
        {
            Events.Add(new BattleEvent
            {
                Type = BattleEventType.Spell,
                ActorName = user.Name,
                TargetName = target.Name,
                Message = abilityName,
                Damage = damage
            });
        }
    }
}