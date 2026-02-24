using gaaameee.Core.Entities;

namespace gaaameee.Core.Interfaces
{
    public interface IBattleField
    {
        BattleResult StartBattle(IArmy army1, IArmy army2);
    }
}