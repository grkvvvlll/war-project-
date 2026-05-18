using Core.Interfaces;
using Core.Formations;

namespace Services.UI
{
    public class UnitRenumberer
    {
        public void Renumber(IArmy army, bool isArmy1, IBattleFormation formation)
        {
            var aliveUnits = army.Units.Where(u => u.IsAlive).ToList();
            bool isWall = formation is WallFormation;

            if (isArmy1 && !isWall)
            {
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[aliveUnits.Count - 1 - i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
            else
            {
                for (int i = 0; i < aliveUnits.Count; i++)
                {
                    var unit = aliveUnits[i];
                    var unitType = unit.Name.Split(' ')[0];
                    unit.Name = $"{unitType} {i + 1}";
                }
            }
        }
    }
}