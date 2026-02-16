using System;

class Program
{
    static void Main()
    {
        Console.Write("Введите текст (латиница, пробелы): ");
        string? input = Console.ReadLine();

        Console.Write("Введите сдвиг: ");
        int shift = int.Parse(Console.ReadLine() ?? "0");

        string encrypted = CaesarLegacy.Encrypt(input ?? "", shift);
        string decrypted = CaesarLegacy.Decrypt(encrypted, shift);

        Console.WriteLine("\nЗашифровано: " + encrypted);
        Console.WriteLine("Расшифровано: " + decrypted);

        Console.WriteLine(input == decrypted ? "Успешно" : "Ошибка");
    }
}

static class CaesarLegacy
{
    // KISS: два алфавита, дальше будет много одинаковых операций
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // YAGNI: метод “для красоты”, но для решения задачи не нужен
    public static void DisplayAlphabet()
    {
        Console.WriteLine(Lower);
        Console.WriteLine(Upper);
    }

    public static string Encrypt(string text, int shift)
    {
        // вроде проверка есть, но код становится длиннее и дублируется
        if (string.IsNullOrEmpty(text))
            return "";

        string result = "";

        foreach (char c in text)
        {
            if (c == ' ')
            {
                result += ' '; // DRY + неэффективно (конкатенация в цикле)
                continue;
            }

            // KISS: две ветки (upper/lower) почти одинаковые
            int idxLower = Lower.IndexOf(c);
            int idxUpper = Upper.IndexOf(c);

            if (idxLower >= 0)
            {
                int newIndex = (idxLower + shift) % Lower.Length;
                if (newIndex < 0) newIndex += Lower.Length;
                result += Lower[newIndex];
            }
            else if (idxUpper >= 0)
            {
                int newIndex = (idxUpper + shift) % Upper.Length;
                if (newIndex < 0) newIndex += Upper.Length;
                result += Upper[newIndex];
            }
            else
            {
                // непонятно что поддерживается — падаем
                throw new Exception("Недопустимый символ: " + c);
            }
        }

        return result;
    }

    // DRY: отдельный метод с почти той же логикой (можно было переиспользовать)
    public static string Decrypt(string text, int shift)
    {
        return Encrypt(text, -shift);
    }
}
