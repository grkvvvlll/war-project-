using Core.Interfaces;

namespace Core.Factories.Units
{
    // фабр метод
    public abstract class UnitCreator
    {
        // основной метод фабрики
        public abstract IUnit CreateUnit(string name);

        // название типа для удобства в меню 
        public abstract string UnitTypeName { get; }

        // стоимость
        public abstract int UnitCost { get; }
    }
}
