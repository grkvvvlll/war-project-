using Core.Interfaces;
using Core.Entities.Units;

namespace Core.Factories.Units
{
    public class LightUnitCreator : UnitCreator
    {
        private readonly IRandomService _random; // Добавлено поле

        public LightUnitCreator(IRandomService random) // Добавлен конструктор
        {
            _random = random;
        }

        public override string UnitTypeName => "Light";
        public override int UnitCost => UnitFactory.LightCost;

        public override IUnit CreateUnit(string name)
        {
            var unit = new LightUnit(
                name,
                UnitFactory.LightAttack,
                UnitFactory.LightDefence,
                UnitFactory.LightHP,
                UnitFactory.LightCost,
                _random); // Передаем random
            return unit;
        }
    }
}