using Core.Entities;

namespace Core.Interfaces
{
    public interface IBattleField
    {
        BattleResult StartBattle(
            IArmy army1,
            IArmy army2,
            int turns = 0,
            bool? army1Turn = null,
            int scoreArmy1 = 0,
            int scoreArmy2 = 0);
    }
}