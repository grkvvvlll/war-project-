using System.Linq;
using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace Services.Battle
{
    public class ArcherPhaseService : IArcherPhaseService
    {
        private readonly IBattleLogger _logger;

        private const int DistanceBetweenArmies = 1;

        public ArcherPhaseService(IBattleLogger logger)
        {
            _logger = logger;
        }

        public int Execute(
            IArmy army,
            IArmy enemy,
            bool isArmy1)
        {
            if (!army.Units.Any(u => u.IsAlive) ||
                !enemy.Units.Any(u => u.IsAlive))
                return 0;

            bool anyArcher = false;
            int totalScore = 0;

            int frontIndex = -1;

            if (isArmy1)
            {
                for (int i = army.Units.Count - 1; i >= 0; i--)
                {
                    if (army.Units[i].IsAlive)
                    {
                        frontIndex = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < army.Units.Count; i++)
                {
                    if (army.Units[i].IsAlive)
                    {
                        frontIndex = i;
                        break;
                    }
                }
            }

            if (frontIndex == -1)
                return 0;

            if (isArmy1)
            {
                for (int i = frontIndex - 1; i >= 0; i--)
                {
                    if (army.Units[i].IsAlive &&
                        army.Units[i] is Archer archer)
                    {
                        anyArcher = true;
                        totalScore += ProcessShot(
                            army, enemy, archer, i, true);
                    }
                }
            }
            else
            {
                for (int i = frontIndex + 1;
                     i < army.Units.Count; i++)
                {
                    if (army.Units[i].IsAlive &&
                        army.Units[i] is Archer archer)
                    {
                        anyArcher = true;
                        totalScore += ProcessShot(
                            army, enemy, archer, i, false);
                    }
                }
            }

            if (!anyArcher)
                _logger.LogNoArchers(army.Name);

            return totalScore;
        }

        private int ProcessShot(
            IArmy army,
            IArmy enemy,
            Archer archer,
            int archerIndex,
            bool isArmy1)
        {
            int frontIndex = -1;

            if (isArmy1)
            {
                for (int i = army.Units.Count - 1; i >= 0; i--)
                {
                    if (army.Units[i].IsAlive)
                    {
                        frontIndex = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < army.Units.Count; i++)
                {
                    if (army.Units[i].IsAlive)
                    {
                        frontIndex = i;
                        break;
                    }
                }
            }

            int aliveBetween = 0;

            if (isArmy1)
            {
                for (int i = archerIndex + 1;
                     i <= frontIndex; i++)
                    if (army.Units[i].IsAlive)
                        aliveBetween++;
            }
            else
            {
                for (int i = frontIndex;
                     i < archerIndex; i++)
                    if (army.Units[i].IsAlive)
                        aliveBetween++;
            }

            int distance =
                aliveBetween + DistanceBetweenArmies;

            _logger.LogArcherShot(
                archer,
                archer.Range,
                distance,
                isArmy1);

            if (archer.Range < distance)
            {
                _logger.LogArrowMiss();
                return 0;
            }

            var target =
                enemy.Units.FirstOrDefault(u => u.IsAlive);

            if (target == null)
                return 0;

            int oldHp = target.Health;
            int damage =
                System.Math.Max(0, archer.Attack - target.Defence);

            target.TakeDamage(damage);

            _logger.LogArcherHit(
                archer,
                target,
                oldHp,
                target.Health,
                isArmy1);

            if (!target.IsAlive)
            {
                _logger.LogDeath(target, !isArmy1);
                return target.Cost;   //  начисление очков
            }

            return 0;
        }
    }
}