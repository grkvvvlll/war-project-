using Core.Entities.Units;
using Core.Formations;
using Core.Interfaces;

namespace Core.Entities.Abilities
{
    public class CloneAbility : ISpecialAbility
    {
        private readonly int _range;
        private readonly int _baseChance;
        private int _currentChance;
        private readonly IRandomService _random;

        public event Action<IUnit, IUnit>? CloneCreated;

        public string Name => "Клонирование";
        public string Description => $"С вероятностью {_baseChance}% создаёт клона союзника";

        public CloneAbility(int range, int baseChance, IRandomService random)
        {
            _range = range;
            _baseChance = baseChance;
            _currentChance = baseChance;
            _random = random;
        }

        public bool CanUse(IUnit user) => user.IsAlive;

        public bool CanTarget(IUnit user, IUnit target, bool isAlly)
        {
            if (!isAlly || !target.IsAlive)
                return false;
            return target is LightUnit || target is Archer;
        }

        public void Use(IUnit user, IUnit target, int distance)
        {
            if (_range < distance)
            {
                _currentChance = Math.Min(100, _currentChance + 5);
                return;
            }

            int roll = _random.Next(0, 100);

            if (roll >= _currentChance)
            {
                _currentChance = Math.Min(100, _currentChance + 5);
                return;
            }

            if (target is ICanBeCloned cloneable)
            {
                var clone = cloneable.Clone(_random);
                CloneTargetName = target.Name;
                CloneCreated?.Invoke(user, clone);
            }

            ResetCharge();
        }

        public string? CloneTargetName { get; private set; }

        public void ResetCharge() => _currentChance = _baseChance;
        public void Charge() => _currentChance = Math.Min(100, _currentChance + 5);

        public int Execute(IUnit user, int userIndex,
                           IArmy ownArmy, IArmy enemyArmy,
                           bool isArmy1, IAbilityExecutionContext ctx)
        {
            ctx.Logger.LogCloneChance(user, GetCurrentChance(), isArmy1);

            // Ищем кандидатов для клонирования среди союзников в радиусе
            var candidates = new List<(IUnit unit, int dist)>();
            for (int j = 0; j < ownArmy.Units.Count; j++)
            {
                var ally = ownArmy.Units[j];
                if (!ally.IsAlive || ally == user) continue;
                if (!(ally is LightUnit || ally is Archer)) continue;
                int dist = ctx.GetAllyDistance(ownArmy, userIndex, j, isArmy1);
                if (dist <= _range)
                    candidates.Add((ally, dist));
            }

            if (!candidates.Any())
            {
                Charge();
                ctx.Logger.LogCloneFailed(user, GetCurrentChance(), isArmy1);
                return 0;
            }

            var (target, targetDist) = candidates[ctx.Random.Next(0, candidates.Count)];

            if (!CanTarget(user, target, isAlly: true))
            {
                Charge();
                ctx.Logger.LogCloneFailed(user, GetCurrentChance(), isArmy1);
                return 0;
            }

            string targetName = target.Name;

            Action<IUnit, IUnit>? handler = null;
            handler = (wizardUser, clone) =>
            {
                int insertPosition = ctx.Formation is WideBridgeFormation
                    ? userIndex
                    : (isArmy1 ? userIndex + 1 : userIndex);

                ownArmy.InsertUnit(clone, insertPosition);
                ctx.RegisterNewUnit(clone);
                ctx.Logger.LogCloneSuccess(wizardUser, targetName, isArmy1);
            };

            CloneCreated += handler;
            Use(user, target, targetDist);
            CloneCreated -= handler;

            if (GetCurrentChance() > _baseChance)
                ctx.Logger.LogCloneFailed(user, GetCurrentChance(), isArmy1);

            return 0;
        }

        //  Метод для получения текущей вероятности
        public int GetCurrentChance() => _currentChance;
        public int GetBaseChance() => _baseChance;
    }
}