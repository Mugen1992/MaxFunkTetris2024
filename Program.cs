using System;

namespace MaxFunkTetris2024
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Console Tetris!");
            Console.WriteLine("Use the arrow keys to control the tetrominoes.");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);

            Game game = new Game(15, 20);
            game.Run();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        public class Tetromino
        {
            // Форма тетромино, представленная двумерным массивом
            public int[,] Shape { get; private set; }

            // Координаты левого верхнего угла тетромино на игровом поле
            public int X { get; set; }
            public int Y { get; set; }

            // Статический массив, содержащий все возможные формы тетромино
            private static readonly int[][,] Shapes = new int[][,]
            {
        new int[,] { { 1, 1 }, { 1, 1 } },  // O-форма
        new int[,] { { 1, 1, 1, 1 } },      // I-форма
        new int[,] { { 0, 1, 1 }, { 1, 1, 0 } },  // Z-форма
        new int[,] { { 1, 1, 0 }, { 0, 1, 1 } },  // S-форма
        new int[,] { { 1, 0, 0 }, { 1, 1, 1 } },  // L-форма
        new int[,] { { 0, 0, 1 }, { 1, 1, 1 } },  // J-форма
        new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }   // T-форма
            };

            // Конструктор, создающий новый тетромино
            public Tetromino(int shapeIndex)
            {
                // Копируем форму из статического массива
                Shape = (int[,])Shapes[shapeIndex].Clone();

                // Устанавливаем начальную позицию тетромино в верхней части поля
                X = 5 - Shape.GetLength(1) / 2;
                Y = 0;
            }

            // Метод для вращения тетромино
            public void Rotate()
            {
                // Создаем новый массив с инвертированными размерами
                int[,] rotated = new int[Shape.GetLength(1), Shape.GetLength(0)];

                // Заполняем новый массив, поворачивая исходную форму на 90 градусов
                for (int i = 0; i < Shape.GetLength(0); i++)
                    for (int j = 0; j < Shape.GetLength(1); j++)
                        rotated[j, Shape.GetLength(0) - 1 - i] = Shape[i, j];

                // Заменяем исходную форму на повернутую
                Shape = rotated;
            }
        }

        public class GameBoard
        {
            // Сетка игрового поля
            public int[,] Grid { get; private set; }

            // Размеры игрового поля
            public int Width { get; }
            public int Height { get; }

            // Конструктор, создающий новое игровое поле
            public GameBoard(int width, int height)
            {
                Width = width;
                Height = height;
                Grid = new int[height, width];
            }

            // Метод для проверки столкновения тетромино с границами поля или другими блоками
            public bool IsCollision(Tetromino tetromino)
            {
                for (int i = 0; i < tetromino.Shape.GetLength(0); i++)
                {
                    for (int j = 0; j < tetromino.Shape.GetLength(1); j++)
                    {
                        if (tetromino.Shape[i, j] == 1)
                        {
                            int x = tetromino.X + j;
                            int y = tetromino.Y + i;
                            // Проверяем выход за границы поля или наличие блока в сетке
                            if (x < 0 || x >= Width || y >= Height || (y >= 0 && Grid[y, x] == 1))
                                return true;
                        }
                    }
                }
                return false;
            }

            // Метод для слияния тетромино с игровым полем
            public void MergeTetromino(Tetromino tetromino)
            {
                for (int i = 0; i < tetromino.Shape.GetLength(0); i++)
                {
                    for (int j = 0; j < tetromino.Shape.GetLength(1); j++)
                    {
                        if (tetromino.Shape[i, j] == 1)
                        {
                            int x = tetromino.X + j;
                            int y = tetromino.Y + i;
                            if (y >= 0 && y < Height && x >= 0 && x < Width)
                                Grid[y, x] = 1;
                        }
                    }
                }
            }

            // Метод для очистки заполненных линий
            public void ClearLines()
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    if (IsLineFull(y))
                    {
                        ClearLine(y);
                        ShiftLinesDown(y);
                        y++; // Проверяем ту же строку снова, так как все опустилось
                    }
                }
            }

            // Метод для проверки, заполнена ли линия
            private bool IsLineFull(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Grid[y, x] == 0)
                        return false;
                }
                return true;
            }

            // Метод для очистки линии
            private void ClearLine(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    Grid[y, x] = 0;
                }
            }

            // Метод для сдвига всех линий выше очищенной вниз
            private void ShiftLinesDown(int clearedLine)
            {
                for (int y = clearedLine - 1; y >= 0; y--)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Grid[y + 1, x] = Grid[y, x];
                    }
                }
            }
        }

        public class GameRenderer
        {
            // Статический метод для отрисовки текущего состояния игры
            public static void Render(GameBoard board, Tetromino currentTetromino)
            {
                Console.Clear();
                for (int y = 0; y < board.Height; y++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        bool isTetromino = false;
                        // Проверяем, находится ли текущий тетромино в этой позиции
                        if (currentTetromino != null)
                        {
                            for (int i = 0; i < currentTetromino.Shape.GetLength(0); i++)
                            {
                                for (int j = 0; j < currentTetromino.Shape.GetLength(1); j++)
                                {
                                    if (currentTetromino.Shape[i, j] == 1 &&
                                        x == currentTetromino.X + j &&
                                        y == currentTetromino.Y + i)
                                    {
                                        isTetromino = true;
                                    }
                                }
                            }
                        }

                        // Отрисовываем блок или пустое место
                        if (board.Grid[y, x] == 1 || isTetromino)
                            Console.Write("█");
                        else
                            Console.Write(".");
                    }
                    Console.WriteLine();
                }
            }
        }

        public class Game
        {
            private GameBoard board;
            private Tetromino currentTetromino;
            private Random random;

            public Game(int width, int height)
            {
                board = new GameBoard(width, height);
                random = new Random();
                currentTetromino = CreateNewTetromino(); // Инициализируем здесь
            }

            private Tetromino CreateNewTetromino()
            {
                return new Tetromino(random.Next(7));
            }

            // Основной игровой цикл
            public void Run()
            {
                while (true)
                {
                    // Отрисовываем текущее состояние игры
                    GameRenderer.Render(board, currentTetromino);

                    // Обрабатываем ввод пользователя
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;
                        HandleInput(key);
                    }

                    // Двигаем тетромино вниз
                    if (!MoveTetrominoDown())
                    {
                        // Если движение вниз невозможно, фиксируем тетромино
                        board.MergeTetromino(currentTetromino);
                        board.ClearLines();
                        if (!SpawnNewTetromino())
                        {
                            Console.WriteLine("Game over");
                            break;
                        }
                    }

                    // Пауза перед следующим шагом
                    System.Threading.Thread.Sleep(200);
                }
            }

            // Обработка ввода пользователя
            private void HandleInput(ConsoleKey key)
            {
                switch (key)
                {
                    case ConsoleKey.LeftArrow:
                        MoveTetromino(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        MoveTetromino(1, 0);
                        break;
                    case ConsoleKey.DownArrow:
                        MoveTetrominoDown();
                        break;
                    case ConsoleKey.UpArrow:
                        RotateTetromino();
                        break;
                }
            }

            // Метод для перемещения тетромино
            private bool MoveTetromino(int dx, int dy)
            {
                currentTetromino.X += dx;
                currentTetromino.Y += dy;
                if (board.IsCollision(currentTetromino))
                {
                    // Если произошло столкновение, возвращаем тетромино на прежнее место
                    currentTetromino.X -= dx;
                    currentTetromino.Y -= dy;
                    return false;
                }
                return true;
            }

            // Метод для перемещения тетромино вниз
            private bool MoveTetrominoDown()
            {
                return MoveTetromino(0, 1);
            }

            // Метод для вращения тетромино
            private void RotateTetromino()
            {
                currentTetromino.Rotate();
                if (board.IsCollision(currentTetromino))
                    currentTetromino.Rotate(); // Поворачиваем обратно, если произошло столкновение
            }

            // Метод для создания нового тетромино
            private bool SpawnNewTetromino()
            {
                currentTetromino = new Tetromino(random.Next(7));
                return !board.IsCollision(currentTetromino);
            }
        }

    }
}