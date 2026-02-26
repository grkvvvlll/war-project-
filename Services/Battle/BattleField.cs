using System;
using System.Linq;
using gaaameee.Core.Entities;
using gaaameee.Core.Interfaces;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        private readonly IMeleeService _meleeService;
        private readonly IArcherPhaseService _archerService;
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;

        private int _scoreArmy1 = 0;
        private int _scoreArmy2 = 0;

        public BattleField(
            IMeleeService meleeService,
            IArcherPhaseService archerService,
            IRandomService random,
            IBattleLogger logger)
        {
            _meleeService = meleeService;
            _archerService = archerService;
            _random = random;
            _logger = logger;
        }

        public BattleResult StartBattle(IArmy army1, IArmy army2)
        {
            int turns = 0;

            bool army1Turn = _random.Next(0, 2) == 0;

            _logger.LogInfo(
                $"Первой атакует: {(army1Turn ? army1.Name : army2.Name)}");

            Wait();

            while (HasAlive(army1) && HasAlive(army2))
            {
                BattleVisualizer.PrintArmyLine(army1, army2);
                Console.WriteLine();

                if (army1Turn)
                {
                    // A удар
                    _scoreArmy1 +=
                        _meleeService.Execute(army1, army2, true);
                    Wait();
                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // B ответ
                    _scoreArmy2 +=
                        _meleeService.Execute(army2, army1, false);
                    Wait();
                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // A лучники
                    _scoreArmy1 +=
                        _archerService.Execute(army1, army2, true);
                    Wait();
                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // B лучники
                    _scoreArmy2 +=
                        _archerService.Execute(army2, army1, false);
                    Wait();
                }
                else
                {
                    // B удар
                    _scoreArmy2 +=
                        _meleeService.Execute(army2, army1, false);
                    Wait();
                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // A ответ
                    _scoreArmy1 +=
                        _meleeService.Execute(army1, army2, true);
                    Wait();
                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // B лучники
                    _scoreArmy2 +=
                        _archerService.Execute(army2, army1, false);
                    Wait();
                    if (!HasAlive(army1) || !HasAlive(army2)) break;

                    // A лучники
                    _scoreArmy1 +=
                        _archerService.Execute(army1, army2, true);
                    Wait();
                }

                _logger.LogInfo(
                    $"\nСЧЁТ: {_scoreArmy1} : {_scoreArmy2}");
                Wait();

                army1.RemoveDeadUnits();
                army2.RemoveDeadUnits();

                turns++;
            }

            string winner = HasAlive(army1)
                ? army1.Name
                : army2.Name;

            return new BattleResult(winner, turns);
        }

        private bool HasAlive(IArmy army)
        {
            return army.Units.Any(u => u.IsAlive);
        }

        private void Wait()
        {
            _logger.LogInfo("Нажмите Enter...");
            Console.ReadLine();
        }
    }
}