using System;
using System.Text;

namespace MaxFunkTetris2024
{
    internal class Program
    {
        // Минимальный рекомендуемый размер консольного окна для корректного UI
        private const int MinConsoleWidth = 80;
        private const int MinConsoleHeight = 30;

        static void Main(string[] args)
        {
            // На старте включаем UTF-8, чтобы рамки и символы отображались корректно
            Console.OutputEncoding = Encoding.UTF8;

            // Один раз предупреждаем, если окно слишком маленькое для интерфейса
            if (Console.WindowWidth < MinConsoleWidth || Console.WindowHeight < MinConsoleHeight)
            {
                Console.Clear();
                Console.WriteLine($"[WARN] Current console size is {Console.WindowWidth}x{Console.WindowHeight}.");
                Console.WriteLine($"Recommended minimum size is {MinConsoleWidth}x{MinConsoleHeight}.");
                Console.WriteLine("Please resize the window and press any key to continue...");
                Console.ReadKey(true);
            }

            Game game = new Game(15, 20);
            game.Run();
        }
    }
}
