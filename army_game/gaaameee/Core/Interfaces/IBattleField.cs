using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    public interface IBattleField
    {
        // Запускает бой между двумя армиями.
        // Бой продолжается, пока в одной из армий не закончатся живые юниты.
        BattleResult StartBattle(IArmy army1, IArmy army2);

        // Выполнить один ход боя (атака + ответ + спецспособность).
        void MakeTurn(IArmy army1, IArmy army2);
    }
}