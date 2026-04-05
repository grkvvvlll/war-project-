using Core.Interfaces;

namespace Core.Entities.Units.Proxies
{
    public class LightUnitProxy : LightUnit
    {
        public LightUnitProxy(LightUnit source, IRandomService random) // Добавлен параметр random
            : base(source.Name, source.Attack, source.Defence, source.Health, source.MaxHealth, source.Cost, random) // Передан в base
        {
        }

        public override void TakeDamage(int damage)
        {
            int oldHp = Health;
            bool wasAlive = IsAlive;
            base.TakeDamage(damage);
            UnitProxySupport.AfterDamage(this, damage, oldHp, wasAlive);
        }
    }
}