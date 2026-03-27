namespace Core.Entities.Units.Proxies
{
    public class ArcherProxy : Archer
    {
        public ArcherProxy(Archer source)
            : base(source.Name, source.Attack, source.Defence, source.Health, source.MaxHealth, source.Cost, source.Range)
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