using System;
using System.Text;

class Program
{
    static void Main()
    {
        Console.Write("Введите текст (латиница, пробелы): ");

        string input = Console.ReadLine() ?? "";

        Console.Write("Введите сдвиг: ");
        int shift = int.Parse(Console.ReadLine() ?? "0");

        string encrypted = CaesarCipher.Encrypt(input, shift);
        string decrypted = CaesarCipher.Decrypt(encrypted, shift);

        Console.WriteLine("\nЗашифровано: " + encrypted);
        Console.WriteLine("Расшифровано: " + decrypted);

        Console.WriteLine(string.Equals(input, decrypted) ? "Успешно" : "Ошибка");
    }
}

static class CaesarCipher
{
    // KISS: работаем от 'a' и длины алфавита, не храним строки алфавита
    private const int AlphabetSize = 26;

    public static string Encrypt(string text, int shift)
    {
        // KISS: если пусто — возвращаем пусто, никаких лишних методов
        if (string.IsNullOrEmpty(text))
            return "";

        // DRY + производительность: вместо result += в цикле используем StringBuilder
        var sb = new StringBuilder(text.Length);

        foreach (char c in text)
        {
            // Поддержим пробелы как есть (можно заменить на char.IsWhiteSpace при желании)
            if (c == ' ')
            {
                sb.Append(' ');
                continue;
            }

            // KISS: приводим к нижнему регистру и проверяем диапазон a-z
            char lower = char.ToLowerInvariant(c);

            if (lower < 'a' || lower > 'z')
                throw new Exception($"Недопустимый символ: {c} (разрешены a-z/A-Z и пробел)");

            // Индекс буквы в алфавите: a=0, b=1 ... z=25
            int index = lower - 'a';

            // DRY: нормализуем сдвиг (чтобы не было огромных чисел и минусов)
            int normalizedShift = shift % AlphabetSize;

            int newIndex = (index + normalizedShift) % AlphabetSize;
            if (newIndex < 0) newIndex += AlphabetSize;

            char newLower = (char)('a' + newIndex);

            // KISS: сохраняем исходный регистр
            sb.Append(char.IsUpper(c) ? char.ToUpperInvariant(newLower) : newLower);
        }

        return sb.ToString();
    }

    // DRY: расшифровка — это тот же шифр с отрицательным сдвигом
    public static string Decrypt(string text, int shift) => Encrypt(text, -shift);
}
