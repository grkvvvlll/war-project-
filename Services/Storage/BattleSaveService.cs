using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Services.Storage
{
    public class BattleSaveService
    {
        private readonly string _dir;

        public BattleSaveService(string? savesDir = null)
        {
            _dir = savesDir ?? Path.Combine(AppContext.BaseDirectory, "saves");
            Directory.CreateDirectory(_dir);
        }

        public string Save(BattleSave save)
        {
            save.SavedAtUtc = save.SavedAtUtc == default ? DateTime.UtcNow : save.SavedAtUtc;

            var fileName = $"battle_{save.SavedAtUtc:yyyyMMdd_HHmmss}.json";
            var path = Path.Combine(_dir, fileName);

            var json = JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            return fileName;
        }

        public BattleSave Load(string fileName)
        {
            var path = Path.Combine(_dir, fileName);
            var json = File.ReadAllText(path);

            var save = JsonSerializer.Deserialize<BattleSave>(json);
            if (save == null) throw new InvalidDataException("Не удалось прочитать сохранение (пустой JSON).");

            return save;
        }

        public List<BattleSaveInfo> ListSaves()
        {
            if (!Directory.Exists(_dir)) return new List<BattleSaveInfo>();

            var files = Directory.GetFiles(_dir, "battle_*.json")
                .OrderByDescending(f => f)
                .ToList();

            var result = new List<BattleSaveInfo>();

            foreach (var path in files)
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var save = JsonSerializer.Deserialize<BattleSave>(json);
                    if (save == null) continue;

                    result.Add(new BattleSaveInfo
                    {
                        FileName = Path.GetFileName(path),
                        SavedAtUtc = save.SavedAtUtc,
                        Winner = save.Winner,
                        Turns = save.Turns
                    });
                }
                catch
                {
                    // битые файлы просто пропускаем
                }
            }

            return result;
        }
    }
}