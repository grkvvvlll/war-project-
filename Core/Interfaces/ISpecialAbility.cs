namespace gaaameee.Core.Interfaces
{
    public interface ISpecialAbility
    {
        string Name { get; }
        bool CanUse(IUnit user);
        void Use(IUnit user, IUnit target);
    }
}