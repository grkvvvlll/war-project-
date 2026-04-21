using Core.Entities.Units;
using Core.Interfaces;
using MedievalRussia;

namespace Core.Factories.Units
{
    public class GulyayGorodCreator : UnitCreator
    {
        public override IUnit CreateUnit(string name)
        {
            var original = new GulyayGorod(50, 6);

            var unit = new GulyayGorodAdapter(
                name,
                50,     // здоровье
                6,      // защита
                UnitFactory.GulyayGorodCost,   // стоимость из фабрики
                original);

            return unit;
        }
        public override string UnitTypeName => "GulyayGorod";
        public override int UnitCost => UnitFactory.GulyayGorodCost;
    }
}

