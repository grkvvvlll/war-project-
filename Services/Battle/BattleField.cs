using Core.Entities;
using Core.Interfaces;

namespace Services.Battle
{
    public class BattleField : IBattleField
    {
        private readonly IMeleeService _meleeService;
        private readonly SpecialAbilityService _specialAbilityService;
        private readonly IRandomService _random;
        private readonly IBattleLogger _logger;
        private int _scoreArmy1 = 0;
        private int _scoreArmy2 = 0;

        public BattleField(
            IMeleeService meleeService,
            SpecialAbilityService specialAbilityService,
            IRandomService random,
            IBattleLogger logger)
        {
            _meleeService = meleeService;
            _specialAbilityService = specialAbilityService;
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
                    // 1. Удар Армии 1
                    _scoreArmy1 += _meleeService.Execute(army1, army2, true);

                    // 2. Ответ Армии 2
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _meleeService.Execute(army2, army1, false);

                    // 3. Special Abilities Армии 1
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _specialAbilityService.Execute(army1, army2, true);

                    // 4. Special Abilities Армии 2
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _specialAbilityService.Execute(army2, army1, false);
                }
                else
                {
                    // 1. Удар Армии 2
                    _scoreArmy2 += _meleeService.Execute(army2, army1, false);

                    // 2. Ответ Армии 1
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _meleeService.Execute(army1, army2, true);

                    // 3. Special Abilities Армии 2
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy2 += _specialAbilityService.Execute(army2, army1, false);

                    // 4. Special Abilities Армии 1
                    if (HasAlive(army1) && HasAlive(army2))
                        _scoreArmy1 += _specialAbilityService.Execute(army1, army2, true);
                }

                _logger.LogInfo($"СЧЁТ: {_scoreArmy1} : {_scoreArmy2}");

                Wait();

                army1.RemoveDeadUnits();
                army2.RemoveDeadUnits();
                turns++;
            }

            string winner = HasAlive(army1) ? army1.Name : army2.Name;
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