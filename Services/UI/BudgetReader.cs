namespace Services.UI
{
    public class BudgetReader
    {
        public int Read()
        {
            while (true)
            {
                Console.Write("Введите бюджет для армий: ");
                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                    return value;
                Console.WriteLine("Введите корректное положительное число.");
            }
        }
    }
}