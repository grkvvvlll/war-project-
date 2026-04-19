using Core.Entities.Units;
using Core.Formations;
using Core.Interfaces;

namespace Services.Battle
{
    public static class BattleVisualizer
    {
        public static void PrintArmyLine(IArmy army1, IArmy army2, IBattleFormation formation)
        {
            if (formation is WideBridgeFormation wideBridge)
            {
                PrintWideBridge(army1, army2, wideBridge);
            }
            else
            {
                PrintBridge(army1, army2);
            }
        }

        // ── Старое построение: одна строка ────────────────────────────────────
        private static void PrintBridge(IArmy army1, IArmy army2)
        {
            // иконки
            foreach (var u in army1.Units)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(GetIcon(u) + " ");
                Console.ResetColor();
            }
            Console.Write("     ");
            foreach (var u in army2.Units)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(GetIcon(u) + " ");
                Console.ResetColor();
            }
            Console.WriteLine();

            // человечки
            foreach (var u in army1.Units)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(u.IsAlive ? "👤 " : "·  ");
                Console.ResetColor();
            }
            Console.Write("     ");
            foreach (var u in army2.Units)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(u.IsAlive ? "👤 " : "·  ");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        // ── Новое построение: 3 строки × N столбцов ───────────────────────────
        private static void PrintWideBridge(IArmy army1, IArmy army2, WideBridgeFormation formation)
        {
            // Получаем карты позиций: unitIndex → (row, col)
            var map1 = formation.GetAlivePositionMap(army1, isArmy1: true);
            var map2 = formation.GetAlivePositionMap(army2, isArmy1: false);

            // Считаем количество столбцов в каждой армии
            int cols1 = map1.Any() ? map1.Values.Max(p => p.col) + 1 : 0;
            int cols2 = map2.Any() ? map2.Values.Max(p => p.col) + 1 : 0;

            // Строим обратную карту: (row, col) → unitIndex для быстрого поиска
            var grid1 = map1.ToDictionary(kv => kv.Value, kv => kv.Key);
            var grid2 = map2.ToDictionary(kv => kv.Value, kv => kv.Key);

            // Армия 1: фронт справа, тыл слева → рисуем столбцы от тыла к фронту
            // Армия 2: фронт слева, тыл справа → рисуем столбцы от фронта к тылу

            for (int row = 0; row < 3; row++)
            {
                // Армия 1: столбцы от максимального (тыл) до 0 (фронт)
                for (int col = cols1 - 1; col >= 0; col--)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    if (grid1.TryGetValue((row, col), out int idx1))
                        Console.Write(GetIcon(army1.Units[idx1]) + " ");
                    else
                        Console.Write("   "); // пустая клетка
                    Console.ResetColor();
                }

                Console.Write("     "); // зазор между армиями

                // Армия 2: столбцы от 0 (фронт) до максимального (тыл)
                for (int col = 0; col < cols2; col++)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (grid2.TryGetValue((row, col), out int idx2))
                        Console.Write(GetIcon(army2.Units[idx2]) + " ");
                    else
                        Console.Write("   ");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            Console.WriteLine(); // отступ после сетки
        }

        private static string GetIcon(IUnit unit)
        {
            IUnit current = unit;
            while (current is Core.Entities.Buffs.UnitDecorator decorator)
                current = decorator.GetInnerUnit();

            return current switch
            {
                Archer _ => "🏹",
                HeavyUnit _ => "🛡️",
                LightUnit _ => "⚔️",
                Wizard _ => "🔮",
                Healer _ => "💚",
                GulyayGorodAdapter _ => "🏰",
                _ => "❓"
            };
        }
    }
}