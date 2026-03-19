using Core.Entities;

namespace Core.Interfaces
{
    public interface IBattleField
    {
        BattleResult StartBattle(IArmy army1, IArmy army2);
    }
}