using System.Collections.Generic;
using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    // Cписок юнитов, управление текущим бойцом и общая стоимость.
    public interface IArmy
    {
        // Все юниты армии (включая мертвых).
        IReadOnlyList<IUnit> Units { get; }

        // Есть ли в армии живые юниты.
        bool HasAliveUnits { get; }

        // Текущий живой юнит, который участвует в бою.
        IUnit GetCurrentAliveUnit();

        // Переключиться на следующего живого юнита.
        void MoveToNextAliveUnit();

        // Получить юнита по позиции (для лучников и проверки дальности).
        IUnit GetUnitAtPosition(int index);

        // Общая стоимость армии (Cost всех юнитов).
        int TotalCost { get; }
    }
}