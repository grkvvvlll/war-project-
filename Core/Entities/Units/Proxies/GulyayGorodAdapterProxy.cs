using MedievalRussia;

namespace Core.Entities.Units.Proxies
{
    public class GulyayGorodAdapterProxy : GulyayGorodAdapter
    {
        public GulyayGorodAdapterProxy(GulyayGorodAdapter source)
            : base(
                source.Name,
                source.Health,
                source.Defence,
                source.Cost,
                new GulyayGorod(source.Health, source.Defence))
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