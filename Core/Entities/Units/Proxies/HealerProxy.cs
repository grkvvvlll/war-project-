namespace Core.Entities.Units.Proxies
{
    public class HealerProxy : Healer
    {
        public HealerProxy(Healer source)
            : base(source.Name, source.Attack, source.Defence, source.Health, source.MaxHealth, source.Cost, source.HealRange, source.HealPower)
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