using System.Text.Json;
using Core.Interfaces;

namespace Services.Storage
{
    public class BattleStateSnapshotService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false
        };

        private readonly IRandomService _random;
        private readonly BattleSaveService _saveService;

        public BattleStateSnapshotService(IRandomService random, BattleSaveService saveService)
        {
            _random = random;
            _saveService = saveService;
        }

        public BattleSave Capture(
            IArmy army1,
            IArmy army2,
            int turns,
            bool army1Turn,
            int scoreArmy1,
            int scoreArmy2,
            IBattleFormation formation,
            IEnumerable<string> logLines,
            string description = "")
        {
            var save = _saveService.CreateInProgressSave(
                army1,
                army2,
                turns,
                army1Turn,
                scoreArmy1,
                scoreArmy2,
                formation,
                logLines,
                description);

            return Clone(save);
        }

        public BattleResumeData Restore(BattleSave snapshot)
        {
            return _saveService.RestoreBattle(Clone(snapshot), _random);
        }

        private static BattleSave Clone(BattleSave snapshot)
        {
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);
            return JsonSerializer.Deserialize<BattleSave>(json)
                ?? throw new InvalidOperationException("Не удалось скопировать снимок боя.");
        }
    }
}