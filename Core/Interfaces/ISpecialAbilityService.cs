namespace Core.Interfaces
{
    public interface ISpecialAbilityService
    {
        void SetFormation(IBattleFormation formation);
        int Execute(IArmy army, IArmy enemy, bool isArmy1);
    }
}