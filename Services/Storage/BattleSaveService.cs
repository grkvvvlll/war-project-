using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Core.Entities;
using Core.Entities.Buffs;
using Core.Entities.Units;
using Core.Interfaces;
using Core.Formations;

namespace Services.Storage
{
    public class BattleSaveService
    {
        private static readonly BattleSaveService _instance = new BattleSaveService();
        public static BattleSaveService Instance => _instance;

        private readonly string _dir;

        private BattleSaveService(string? savesDir = null)
        {
            _dir = savesDir ?? Path.Combine(AppContext.BaseDirectory, "saves");
            Directory.CreateDirectory(_dir);
        }

        public string Save(BattleSave save, string? customName = null)
        {
            save.SavedAtUtc = save.SavedAtUtc == default ? DateTime.UtcNow : save.SavedAtUtc;

            if (!string.IsNullOrWhiteSpace(customName))
                save.DisplayName = customName.Trim();

            if (string.IsNullOrWhiteSpace(save.DisplayName))
                save.DisplayName = $"Сохранение {save.SavedAtUtc:yyyy-MM-dd HH:mm}";

            string safeName = SanitizeFileName(save.DisplayName);
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = $"battle_{save.SavedAtUtc:yyyyMMdd_HHmmss}";

            var fileName = $"{safeName}_{save.SavedAtUtc:yyyyMMdd_HHmmss}.json";
            var path = Path.Combine(_dir, fileName);

            var json = JsonSerializer.Serialize(
                save,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, json, Encoding.UTF8);
            return fileName;
        }

        public BattleSave Load(string fileName)
        {
            var path = Path.Combine(_dir, fileName);
            var json = File.ReadAllText(path, Encoding.UTF8);

            var save = JsonSerializer.Deserialize<BattleSave>(json);
            if (save == null)
                throw new InvalidDataException("Не удалось прочитать сохранение.");

            return save;
        }

        public List<BattleSaveInfo> ListSaves()
        {
            if (!Directory.Exists(_dir))
                return new List<BattleSaveInfo>();

            var files = Directory.GetFiles(_dir, "*.json")
                .OrderByDescending(f => f)
                .ToList();

            var result = new List<BattleSaveInfo>();

            foreach (var path in files)
            {
                try
                {
                    var json = File.ReadAllText(path, Encoding.UTF8);
                    var save = JsonSerializer.Deserialize<BattleSave>(json);
                    if (save == null)
                        continue;

                    result.Add(new BattleSaveInfo
                    {
                        FileName = Path.GetFileName(path),
                        DisplayName = save.DisplayName,
                        SavedAtUtc = save.SavedAtUtc,
                        Winner = save.Winner,
                        Turns = save.Turns
                    });
                }
                catch
                {
                }
            }

            return result;
        }

        public BattleSave CreateFinishedSave(
            BattleResult result,
            IEnumerable<string> logLines,
            string? displayName = null)
        {
            return new BattleSave
            {
                SavedAtUtc = DateTime.UtcNow,
                DisplayName = displayName ?? "",
                Turns = result.Turns,
                Army1Turn = false,
                ScoreArmy1 = 0,
                ScoreArmy2 = 0,
                IsFinished = true,
                Winner = result.Winner,
                LogLines = logLines.ToList()
            };
        }

        public BattleSave CreateInProgressSave(
            IArmy army1,
            IArmy army2,
            int turns,
            bool army1Turn,
            int scoreArmy1,
            int scoreArmy2,
            IBattleFormation formation,
            IEnumerable<string> logLines,
            string? displayName = null)
        {
            return new BattleSave
            {
                SavedAtUtc = DateTime.UtcNow,
                DisplayName = displayName ?? "",
                Turns = turns,
                Army1Turn = army1Turn,
                ScoreArmy1 = scoreArmy1,
                ScoreArmy2 = scoreArmy2,
                FormationType = GetFormationType(formation),
                Army1 = MapArmy(army1),
                Army2 = MapArmy(army2),
                IsFinished = false,
                Winner = "",
                LogLines = logLines.ToList()
            };
        }

        public BattleResumeData RestoreBattle(BattleSave save, IRandomService random)
        {
            if (save.IsFinished)
            {
                return new BattleResumeData
                {
                    Turns = save.Turns,
                    Army1Turn = save.Army1Turn,
                    ScoreArmy1 = save.ScoreArmy1,
                    ScoreArmy2 = save.ScoreArmy2,
                    Formation = CreateFormation(save.FormationType),
                    IsFinished = true,
                    Winner = save.Winner
                };
            }

            return new BattleResumeData
            {
                Army1 = RestoreArmy(save.Army1, random),
                Army2 = RestoreArmy(save.Army2, random),
                Turns = save.Turns,
                Army1Turn = save.Army1Turn,
                ScoreArmy1 = save.ScoreArmy1,
                ScoreArmy2 = save.ScoreArmy2,
                Formation = CreateFormation(save.FormationType),
                IsFinished = false,
                Winner = save.Winner
            };
        }

        private ArmySnapshot MapArmy(IArmy army)
        {
            return new ArmySnapshot
            {
                Name = army.Name,
                Units = army.Units.Select(MapUnit).ToList()
            };
        }

        private UnitSnapshot MapUnit(IUnit unit)
        {
            var baseUnit = GetBaseUnit(unit);
            var snapshot = new UnitSnapshot
            {
                UnitType = GetUnitType(baseUnit),
                Name = unit.Name,
                Attack = unit.Attack,
                Defence = unit.Defence,
                Health = unit.Health,
                MaxHealth = unit.MaxHealth,
                Cost = unit.Cost
            };

            if (baseUnit is Archer archer)
            {
                snapshot.Range = archer.Range;
            }
            else if (baseUnit is Healer healer)
            {
                snapshot.HealRange = healer.HealRange;
                snapshot.HealPower = healer.HealPower;
            }
            else if (baseUnit is Wizard wizard)
            {
                snapshot.SpellRange = wizard.SpellRange;
                snapshot.ClonePower = wizard.ClonePower;
            }

            return snapshot;
        }

        private IUnit GetBaseUnit(IUnit unit)
        {
            while (unit is UnitDecorator decorator)
                unit = decorator.GetInnerUnit();

            return unit;
        }

        private string GetUnitType(IUnit unit)
        {
            if (unit is HeavyUnit) return "Heavy";
            if (unit is LightUnit) return "Light";
            if (unit is Archer) return "Archer";
            if (unit is Healer) return "Healer";
            if (unit is Wizard) return "Wizard";
            if (unit is GulyayGorodAdapter) return "GulyayGorod";

            throw new InvalidDataException($"Неизвестный тип юнита: {unit.GetType().Name}");
        }

        private IArmy RestoreArmy(ArmySnapshot snapshot, IRandomService random)
        {
            var units = snapshot.Units
                .Select(unit => RestoreUnit(unit, random))
                .ToList();

            return new Core.Entities.Army(snapshot.Name, units);
        }

        private IUnit RestoreUnit(UnitSnapshot dto, IRandomService random)
        {
            return dto.UnitType switch
            {
                "Heavy" => new HeavyUnit(dto.Name, dto.Attack, dto.Defence, dto.Health, dto.MaxHealth, dto.Cost),

                "Light" => new LightUnit(dto.Name, dto.Attack, dto.Defence, dto.Health, dto.MaxHealth, dto.Cost, random),

                "Archer" => new Archer(dto.Name, dto.Attack, dto.Defence, dto.Health, dto.MaxHealth, dto.Cost, dto.Range ?? 0),

                "Healer" => new Healer(dto.Name, dto.Attack, dto.Defence, dto.Health, dto.MaxHealth, dto.Cost, dto.HealRange ?? 0, dto.HealPower ?? 0),

                "Wizard" => new Wizard(dto.Name, dto.Attack, dto.Defence, dto.Health, dto.MaxHealth, dto.Cost, dto.SpellRange ?? 0, dto.ClonePower ?? 0, random),

                "GulyayGorod" => CreateGulyayGorod(dto),

                _ => throw new InvalidDataException($"Неизвестный тип юнита: {dto.UnitType}")
            };
        }
        private string GetFormationType(IBattleFormation formation)
        {
            if (formation is WideBridgeFormation) return "WideBridge";
            if (formation is WallFormation) return "Wall";
            return "Bridge";
        }

        private IBattleFormation CreateFormation(string? formationType)
        {
            return formationType switch
            {
                "WideBridge" => new WideBridgeFormation(),
                "Wall" => new WallFormation(),
                _ => new BridgeFormation()
            };
        }
        private string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var result = new StringBuilder();

            foreach (char c in name)
            {
                if (!invalid.Contains(c))
                    result.Append(c);
            }

            return result.ToString().Trim().Replace(" ", "_");
        }

        private IUnit CreateGulyayGorod(UnitSnapshot dto)
        {
            var original = new MedievalRussia.GulyayGorod(dto.Health, dto.Defence);

            var unit = new GulyayGorodAdapter(
                dto.Name,
                dto.Health,
                dto.Defence,
                dto.Cost,
                original);

            return unit;
        }
    }
}
