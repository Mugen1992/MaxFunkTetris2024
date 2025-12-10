using System;

namespace MaxFunkTetris2024
{
    // Состояния игры для управления основными экранами
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(15, 20);
            game.Run();
        }

        public class Tetromino
        {
            // Форма тетрамино, представленная двумерным массивом
            public int[,] Shape { get; private set; }

            // Координаты левого верхнего угла тетрамино на игровом поле
            public int X { get; set; }
            public int Y { get; set; }

            // Статический массив, содержащий все возможные формы тетрамино
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

            // Конструктор, создающий новый тетрамино
            public Tetromino(int shapeIndex)
            {
                // Копируем форму из статического массива
                Shape = (int[,])Shapes[shapeIndex].Clone();

                // Устанавливаем начальную позицию тетрамино в верхней части поля
                X = 5 - Shape.GetLength(1) / 2;
                Y = 0;
            }

            // Метод для вращения тетрамино
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

            // Метод для проверки столкновения тетрамино с границами поля или другими блоками
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

            // Метод для слияния тетрамино с игровым полем
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

            // Метод для очистки заполненных линий и возврата количества очищенных строк
            public int ClearLines()
            {
                int clearedLines = 0;
                for (int y = Height - 1; y >= 0; y--)
                {
                    if (IsLineFull(y))
                    {
                        ClearLine(y);
                        ShiftLinesDown(y);
                        clearedLines++;
                        y++; // Проверяем ту же строку снова, так как все опустилось
                    }
                }
                return clearedLines;
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
                        // Проверяем, находится ли текущий тетрамино в этой позиции
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
            private int _score;
            private int _tick; // Счётчик кадров для управления падением
            private int _dropInterval = 6; // Интервал падения фигуры в кадрах
            private readonly int _width;
            private readonly int _height;
            private GameState _state;

            public Game(int width, int height)
            {
                _width = width;
                _height = height;
                random = new Random();
                _state = GameState.MainMenu; // Запуск с главного меню
                ResetGame();
            }

            // Метод для сброса игры и подготовки нового запуска
            private void ResetGame()
            {
                board = new GameBoard(_width, _height);
                currentTetromino = CreateNewTetromino();
                _score = 0;
                _tick = 0; // Обнуляем счётчик кадров при запуске новой игры
            }

            // Фабрика для создания случайного тетрамино
            private Tetromino CreateNewTetromino()
            {
                return new Tetromino(random.Next(7));
            }

            // Основной игровой цикл со стейт-машиной
            public void Run()
            {
                const int frameDelayMs = 30; // ~33 FPS фиксированная задержка кадра
                while (true)
                {
                    HandleInput();
                    Update();
                    Render();
                    System.Threading.Thread.Sleep(frameDelayMs);
                }
            }

            // Обработка пользовательского ввода в зависимости от состояния
            private void HandleInput()
            {
                switch (_state)
                {
                    case GameState.MainMenu:
                        HandleInputMainMenu();
                        break;
                    case GameState.Playing:
                        HandleInputPlaying();
                        break;
                    case GameState.Paused:
                        HandleInputPaused();
                        break;
                    case GameState.GameOver:
                        HandleInputGameOver();
                        break;
                }
            }

            // Обновление игрового процесса в зависимости от состояния
            private void Update()
            {
                switch (_state)
                {
                    case GameState.MainMenu:
                        UpdateMainMenu();
                        break;
                    case GameState.Playing:
                        UpdatePlaying();
                        break;
                    case GameState.Paused:
                        UpdatePaused();
                        break;
                    case GameState.GameOver:
                        UpdateGameOver();
                        break;
                }
            }

            // Отрисовка текущего состояния игры
            private void Render()
            {
                switch (_state)
                {
                    case GameState.MainMenu:
                        RenderMainMenu();
                        break;
                    case GameState.Playing:
                        RenderPlaying();
                        break;
                    case GameState.Paused:
                        RenderPaused();
                        break;
                    case GameState.GameOver:
                        RenderGameOver();
                        break;
                }
            }

            // Ввод в главном меню: старт новой игры или выход
            private void HandleInputMainMenu()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
                {
                    ResetGame();
                    _state = GameState.Playing;
                }
                else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
                {
                    Environment.Exit(0);
                }
            }

            // Ввод во время игры: движение и вращение фигур
            private void HandleInputPlaying()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                ConsoleKey key = Console.ReadKey(true).Key;
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

            // Ввод в паузе пока оставляем пустым для будущего расширения
            private void HandleInputPaused()
            {
                // Зарезервировано для логики паузы
            }

            // Ввод после завершения игры: любой ввод возвращает в меню
            private void HandleInputGameOver()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                Console.ReadKey(true);
                _state = GameState.MainMenu;
            }

            // Обновление в главном меню пока не требуется
            private void UpdateMainMenu()
            {
                // Логика обновления меню не нужна, оставляем заглушку
            }

            // Обновление игрового процесса: падение фигур и проверка завершения
            private void UpdatePlaying()
            {
                _tick++; // Увеличиваем счётчик кадров
                bool shouldFall = _tick % _dropInterval == 0; // Проверяем, пора ли падать
                if (!shouldFall) return; // Если ещё рано, выходим

                if (!MoveTetrominoDown())
                {
                    board.MergeTetromino(currentTetromino);
                    int clearedLines = board.ClearLines();
                    if (clearedLines > 0)
                        _score += clearedLines * 100; // Начисляем очки за линии

                    if (!SpawnNewTetromino())
                        _state = GameState.GameOver; // Переход в Game Over при невозможности спавна
                }
            }

            // Обновление в паузе пока оставляем пустым для будущего расширения
            private void UpdatePaused()
            {
                // Зарезервировано для логики паузы
            }

            // Обновление после завершения игры пока не требуется
            private void UpdateGameOver()
            {
                // Логика обновления для экрана Game Over не требуется
            }

            // Отрисовка меню: заголовок и пункты выбора
            private void RenderMainMenu()
            {
                Console.SetCursorPosition(0, 0); // Перемещаем курсор в начало для уменьшения мерцания
                Console.WriteLine("=== MaxFunkTetris2024 ===");
                Console.WriteLine();
                Console.WriteLine("1. Start game");
                Console.WriteLine("2. Exit");
                Console.WriteLine();
                Console.WriteLine("Выберите пункт меню и нажмите соответствующую цифру.");
            }

            // Отрисовка игрового процесса с полем и очками
            private void RenderPlaying()
            {
                GameRenderer.Render(board, currentTetromino);
                Console.WriteLine();
                Console.WriteLine($"Очки: {_score}");
                Console.WriteLine("Управление: ← → для движения, ↑ для вращения, ↓ для ускорения.");
            }

            // Отрисовка паузы пока оставляем пустой
            private void RenderPaused()
            {
                // Зарезервировано для будущей визуализации паузы
            }

            // Отрисовка экрана завершения игры
            private void RenderGameOver()
            {
                Console.SetCursorPosition(0, 0); // Обновляем текст без полной очистки экрана
                Console.WriteLine("Game Over");
                Console.WriteLine($"Итоговый счёт: {_score}");
                Console.WriteLine();
                Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в главное меню.");
            }

            // Метод для перемещения тетрамино
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

            // Метод для перемещения тетрамино вниз
            private bool MoveTetrominoDown()
            {
                return MoveTetromino(0, 1);
            }

            // Метод для вращения тетрамино
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