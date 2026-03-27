namespace Core.Entities.Units.Proxies
{
    public class HeavyUnitProxy : HeavyUnit
    {
        public HeavyUnitProxy(HeavyUnit source)
            : base(source.Name, source.Attack, source.Defence, source.Health, source.MaxHealth, source.Cost)
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