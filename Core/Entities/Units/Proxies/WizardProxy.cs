using Core.Interfaces;

namespace Core.Entities.Units.Proxies
{
    public class WizardProxy : Wizard
    {
        public WizardProxy(Wizard source, IRandomService random)
            : base(source.Name, source.Attack, source.Defence, source.Health, source.MaxHealth, source.Cost, source.SpellRange, source.ClonePower, random)
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