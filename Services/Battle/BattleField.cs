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

            // Пауза перед началом боя (остаётся)
            Wait();

            while (HasAlive(army1) && HasAlive(army2))
            {
                // Визуализация в начале раунда
                BattleVisualizer.PrintArmyLine(army1, army2);
                Console.WriteLine();

                if (army1Turn)
                {
                    // 1. Удар Армии 1
                    _scoreArmy1 += _meleeService.Execute(army1, army2, true);

                    // 2. Ответ Армии 2 (если кто-то жив)
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _meleeService.Execute(army2, army1, false);

                    // 3. Лучники Армии 1
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _archerService.Execute(army1, army2, true);

                    // 4. Лучники Армии 2
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _archerService.Execute(army2, army1, false);
                }
                else
                {
                    // 1. Удар Армии 2
                    _scoreArmy2 += _meleeService.Execute(army2, army1, false);

                    // 2. Ответ Армии 1
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _meleeService.Execute(army1, army2, true);

                    // 3. Лучники Армии 2
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _archerService.Execute(army2, army1, false);

                    // 4. Лучники Армии 1
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _archerService.Execute(army1, army2, true);
                }

                // Логирование счета
                _logger.LogInfo($"СЧЁТ: {_scoreArmy1} : {_scoreArmy2}");

                // ЕДИНСТВЕННАЯ ПАУЗА НА РАУНД
                Wait();

                // Очистка мертвых юнитов в конце раунда
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
            _logger.LogInfo("Нажмите Enter для следующего раунда...");
            Console.ReadLine();
        }
    }
}