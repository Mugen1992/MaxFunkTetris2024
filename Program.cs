using System;
using System.Diagnostics;
using System.Text;

namespace MaxFunkTetris2024
{
    // Состояния игры для управления основными экранами
    public enum GameState
    {
        MainMenu,
        Help,
        Settings,
        Leaderboard,
        Playing,
        Paused,
        GameOver
    }

    public enum PowerUpType
    {
        None,
        Bomb,       // Очищает 3 нижних ряда
        Freeze      // Замедляет падение на несколько ходов
    }

    // Уровни сложности для настройки скорости игры
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

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

            // Сохраняем снимок текущей формы и координат, чтобы можно было вернуть состояние
            public (int[,] shape, int x, int y) SaveState()
            {
                return ((int[,])Shape.Clone(), X, Y);
            }

            // Возвращаем ранее сохранённую форму и позицию
            public void RestoreState(int[,] shape, int x, int y)
            {
                Shape = shape;
                X = x;
                Y = y;
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

                // После сдвига очищаем верхнюю строку, чтобы не тянуть «мусор» сверху
                for (int x = 0; x < Width; x++)
                {
                    Grid[0, x] = 0;
                }
            }
        }

        public class GameRenderer
        {
            // Статический метод для отрисовки текущего состояния игры
            public static void Render(GameBoard board, Tetromino currentTetromino)
            {
                // Рисуем построчно, чтобы избежать полной очистки консоли и мерцания
                for (int y = 0; y < board.Height; y++)
                {
                    char[] row = new char[board.Width];

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
                        row[x] = (board.Grid[y, x] == 1 || isTetromino) ? '█' : '.';
                    }

                    Console.SetCursorPosition(UiLayout.BoardOffsetX, UiLayout.BoardOffsetY + y);
                    Console.Write(new string(row));
                }

                // Переводим курсор под поле, чтобы последующие WriteLine не попадали внутрь поля
                Console.SetCursorPosition(UiLayout.BoardOffsetX, UiLayout.BoardOffsetY + board.Height);
            }
        }

        public static class UiLayout
        {
            // Базовые размеры и смещения элементов интерфейса
            public const int LeftPanelX = 1;
            public const int LeftPanelY = 1;
            public const int LeftPanelWidth = 16;
            public const int LeftPanelHeight = 9;

            public const int BoardOffsetX = LeftPanelX + LeftPanelWidth + 3;
            public const int BoardOffsetY = 1;

            public const int PanelPadding = 3;
            public const int RightPanelWidth = 28;
            public const int RightPanelHeight = 9;

            public const int NextPanelWidth = 16;
            public const int NextPanelHeight = 6;

            public const int InventoryPanelWidth = 16;
            public const int InventoryPanelHeight = 5;

            // Координаты подписей статистики
            public static int StatsLabelX => LeftPanelX + 2;
            public static int StatsValueX => LeftPanelX + LeftPanelWidth - 7; // Сдвигаем левее, чтобы 6-значные числа не затирали рамку
            public static int ScoreLabelY => LeftPanelY + 2;
            public static int LevelLabelY => LeftPanelY + 3;
            public static int LinesLabelY => LeftPanelY + 4;

            // Координаты панелей, зависящие от ширины поля
            public static int GetInfoPanelX(int boardWidth) => BoardOffsetX + boardWidth + PanelPadding;
            public static int InfoPanelY => LeftPanelY;

            public static int GetNextPanelX(int boardWidth) => GetInfoPanelX(boardWidth);
            public static int GetNextPanelY() => InfoPanelY + RightPanelHeight + 1;

            public static int GetInventoryPanelX(int boardWidth) => GetNextPanelX(boardWidth);
            public static int GetInventoryPanelY() => GetNextPanelY() + NextPanelHeight + 1;
        }

        public static class UiTheme
        {
            // Символы рамки
            public const char HorizontalBorder = '─';
            public const char VerticalBorder = '│';
            public const char CornerTopLeft = '┌';
            public const char CornerTopRight = '┐';
            public const char CornerBottomLeft = '└';
            public const char CornerBottomRight = '┘';

            // Цвета элементов интерфейса
            public const ConsoleColor FrameColor = ConsoleColor.DarkGray;
            public const ConsoleColor LabelColor = ConsoleColor.Gray;
            public const ConsoleColor ValueColor = ConsoleColor.Cyan;
            public const ConsoleColor InfoColor = ConsoleColor.White;
        }

        public static class UiFrameRenderer
        {
            // Универсальный метод для рисования прямоугольной рамки
            public static void DrawBox(int x, int y, int width, int height, string title = "")
            {
                // Сохраняем текущий цвет, чтобы вернуть его после отрисовки
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.FrameColor;

                // Верхняя грань
                Console.SetCursorPosition(x, y);
                Console.Write(UiTheme.CornerTopLeft);
                Console.Write(new string(UiTheme.HorizontalBorder, width - 2));
                Console.Write(UiTheme.CornerTopRight);

                // Боковые грани
                for (int i = 1; i < height - 1; i++)
                {
                    Console.SetCursorPosition(x, y + i);
                    Console.Write(UiTheme.VerticalBorder);
                    Console.SetCursorPosition(x + width - 1, y + i);
                    Console.Write(UiTheme.VerticalBorder);
                }

                // Нижняя грань
                Console.SetCursorPosition(x, y + height - 1);
                Console.Write(UiTheme.CornerBottomLeft);
                Console.Write(new string(UiTheme.HorizontalBorder, width - 2));
                Console.Write(UiTheme.CornerBottomRight);

                // Заголовок рисуем один раз поверх рамки
                if (!string.IsNullOrWhiteSpace(title))
                {
                    Console.SetCursorPosition(x + 2, y);
                    Console.ForegroundColor = UiTheme.LabelColor;
                    Console.Write(title);
                }

                Console.ForegroundColor = previousColor;
            }

            // Рисуем все статические рамки и подписи экранов во время игры
            public static void DrawStaticFrame(int boardWidth, int boardHeight)
            {
                Console.Clear();

                // Рисуем рамку вокруг игрового поля
                DrawBox(UiLayout.BoardOffsetX - 1, UiLayout.BoardOffsetY - 1, boardWidth + 2, boardHeight + 2, " FIELD ");

                // Левая панель статистики
                DrawBox(UiLayout.LeftPanelX, UiLayout.LeftPanelY, UiLayout.LeftPanelWidth, UiLayout.LeftPanelHeight, " STATS ");
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.LabelColor;
                Console.SetCursorPosition(UiLayout.StatsLabelX, UiLayout.ScoreLabelY);
                Console.Write("Score:");
                Console.SetCursorPosition(UiLayout.StatsLabelX, UiLayout.LevelLabelY);
                Console.Write("Level:");
                Console.SetCursorPosition(UiLayout.StatsLabelX, UiLayout.LinesLabelY);
                Console.Write("Lines:");

                // Правая панель информации
                int infoPanelX = UiLayout.GetInfoPanelX(boardWidth);
                DrawBox(infoPanelX, UiLayout.InfoPanelY, UiLayout.RightPanelWidth, UiLayout.RightPanelHeight, " INFO ");
                Console.ForegroundColor = UiTheme.InfoColor;
                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 2);
                Console.Write("←/→ — движение");
                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 3);
                Console.Write("↓ — ускорение падения");
                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 4);
                Console.Write("↑ — вращение фигуры");
                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 5);
                Console.Write("P — пауза");
                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 6);
                Console.Write("Esc — выход в меню");
                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 7);
                Console.Write("Enter — подтверждение");

                int nextPanelX = UiLayout.GetNextPanelX(boardWidth);
                int nextPanelY = UiLayout.GetNextPanelY();
                DrawBox(nextPanelX, nextPanelY, UiLayout.NextPanelWidth, UiLayout.NextPanelHeight, " NEXT ");

                int inventoryPanelX = UiLayout.GetInventoryPanelX(boardWidth);
                int inventoryPanelY = UiLayout.GetInventoryPanelY();
                DrawBox(inventoryPanelX, inventoryPanelY, UiLayout.InventoryPanelWidth, UiLayout.InventoryPanelHeight, " ITEM ");

                Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 8);
                Console.Write("B — исп. бонус");

                Console.ForegroundColor = previousColor;
            }

        }

        public static class UiStatsRenderer
        {
            // Обновляем только числовые значения статистики, подписи остаются статичными
            public static void RenderStats(int score, int level, int lines)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.ValueColor;

                Console.SetCursorPosition(UiLayout.StatsValueX, UiLayout.ScoreLabelY);
                Console.Write($"{score,6}");

                Console.SetCursorPosition(UiLayout.StatsValueX, UiLayout.LevelLabelY);
                Console.Write($"{level,6}");

                Console.SetCursorPosition(UiLayout.StatsValueX, UiLayout.LinesLabelY);
                Console.Write($"{lines,6}");

                Console.ForegroundColor = previousColor;
            }

            public static void RenderNextTetromino(Tetromino nextTetromino, int boardWidth)
            {
                int panelX = UiLayout.GetNextPanelX(boardWidth);
                int panelY = UiLayout.GetNextPanelY();

                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.ValueColor;

                // Очистка старой фигуры (просто закрашиваем пробелами внутреннюю область)
                for (int y = 0; y < 4; y++)
                {
                    Console.SetCursorPosition(panelX + 2, panelY + 1 + y);
                    Console.Write(new string(' ', UiLayout.NextPanelWidth - 4));
                }

                if (nextTetromino != null)
                {
                    // Центрируем фигуру внутри панели
                    int shapeWidth = nextTetromino.Shape.GetLength(1);
                    int shapeHeight = nextTetromino.Shape.GetLength(0);
                    int offsetX = (UiLayout.NextPanelWidth - shapeWidth) / 2;
                    int offsetY = (UiLayout.NextPanelHeight - shapeHeight) / 2;

                    for (int i = 0; i < shapeHeight; i++)
                    {
                        for (int j = 0; j < shapeWidth; j++)
                        {
                            if (nextTetromino.Shape[i, j] == 1)
                            {
                                Console.SetCursorPosition(panelX + offsetX + j, panelY + offsetY + i);
                                Console.Write('█');
                            }
                        }
                    }
                }

                Console.ForegroundColor = previousColor;
            }

            public static void RenderInventory(PowerUpType currentPowerUp, int boardWidth)
            {
                int panelX = UiLayout.GetInventoryPanelX(boardWidth);
                int panelY = UiLayout.GetInventoryPanelY();

                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.ValueColor;

                // Очистка старого названия
                Console.SetCursorPosition(panelX + 2, panelY + 2);
                Console.Write(new string(' ', UiLayout.InventoryPanelWidth - 4));

                Console.SetCursorPosition(panelX + 2, panelY + 2);
                switch (currentPowerUp)
                {
                    case PowerUpType.Bomb:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("  [ BOMB ]  ");
                        break;
                    case PowerUpType.Freeze:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(" [ FREEZE ] ");
                        break;
                    case PowerUpType.None:
                    default:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("  ( empty ) ");
                        break;
                }

                Console.ForegroundColor = previousColor;
            }
        }

        public class Game
        {
            // Инициализируем через null!-заглушку, т.к. реальные значения задаются в ResetGame/SpawnNewTetromino
            private GameBoard board = null!;
            private Tetromino currentTetromino = null!;
            private Tetromino nextTetromino = null!;
            private Random random;
            private int _score;
            private int _tick; // Счётчик кадров для управления падением
            private int _dropInterval = 6; // Интервал падения фигуры в кадрах
            private int _linesCleared; // Общее количество убранных линий для статистики
            private readonly int _width;
            private readonly int _height;
            private GameState _state;
            private bool _mainMenuDrawn; // Флаг, чтобы меню рисовалось один раз при входе
            private bool _gameOverDrawn; // Флаг для одноразовой отрисовки экрана Game Over
            private bool _pausedDrawn; // Задел под будущий экран паузы
            private bool _helpDrawn; // Флаг для одноразовой отрисовки экрана помощи
            private bool _settingsDrawn; // Флаг для одноразовой отрисовки экрана настроек
            private int _mainMenuSelectedIndex; // Текущий выделенный пункт главного меню
            private int _settingsSelectedIndex; // Текущий выделенный пункт в настройках сложности

            private Difficulty _difficulty; // Текущая сложность игры

            private PowerUpType _currentPowerUp; // Текущий бонус в инвентаре
            private int _freezeTicksRemaining; // Оставшееся время заморозки

            private readonly string[] _difficultyOptions = { "Easy", "Normal", "Hard" }; // Подписи пунктов сложности

            private readonly string[] _mainMenuItems = { "Start", "Leaderboard", "Help", "Settings", "Exit" }; // Подписи пунктов меню

            private bool _leaderboardDrawn; // Флаг для одноразовой отрисовки экрана лидерборда
            private const string HighscoreFilePath = "highscores.txt";

            // Простейшая метрика кадра для диагностики производительности
            private readonly Stopwatch _frameStopwatch = new();
            private double _lastFrameMilliseconds;
            private double _maxFrameMilliseconds;

            private int CurrentLevel => Math.Max(1, _linesCleared / 10 + 1);

            public Game(int width, int height)
            {
                _width = width;
                _height = height;
                random = new Random();
                _state = GameState.MainMenu; // Запуск с главного меню
                _difficulty = Difficulty.Normal; // Базовая сложность
                _settingsSelectedIndex = (int)_difficulty;
                _dropInterval = GetDropInterval(_difficulty);
                ResetGame();
                _mainMenuDrawn = false;
                _gameOverDrawn = false;
                _pausedDrawn = false;
                _helpDrawn = false;
                _settingsDrawn = false;
                _leaderboardDrawn = false;
                _mainMenuSelectedIndex = 0;
            }

            // Метод для сброса игры и подготовки нового запуска
            private void ResetGame()
            {
                board = new GameBoard(_width, _height);
                nextTetromino = CreateNewTetromino();
                currentTetromino = CreateNewTetromino();
                _score = 0;
                _tick = 0; // Обнуляем счётчик кадров при запуске новой игры
                _linesCleared = 0;
                _currentPowerUp = PowerUpType.None;
                _freezeTicksRemaining = 0;
                _dropInterval = GetDropInterval(_difficulty); // Применяем скорость падения под выбранную сложность
            }

            // Определяем интервал падения для выбранной сложности
            private int GetDropInterval(Difficulty difficulty)
            {
                // Чем сложнее режим, тем ниже интервал и выше скорость
                return difficulty switch
                {
                    Difficulty.Easy => 10,
                    Difficulty.Normal => 6,
                    Difficulty.Hard => 3,
                    _ => 6
                };
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
                // Прячем курсор один раз перед стартом игрового цикла, чтобы не мешал анимации
                Console.CursorVisible = false;
                while (true)
                {
                    // Стартуем замер длительности кадра
                    _frameStopwatch.Restart();

                    HandleInput();
                    Update();
                    Render();

                    // Фиксируем длительность кадра и обновляем простую статистику
                    _frameStopwatch.Stop();
                    _lastFrameMilliseconds = _frameStopwatch.Elapsed.TotalMilliseconds;
                    if (_lastFrameMilliseconds > _maxFrameMilliseconds)
                    {
                        _maxFrameMilliseconds = _lastFrameMilliseconds;
                    }

#if DEBUG
                    // Рисуем диагностику только во время активной игры или паузы, чтобы не портить меню
                    if (_state == GameState.Playing || _state == GameState.Paused)
                    {
                        RenderFrameDiagnostics();
                    }
#endif
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
                    case GameState.Leaderboard:
                        HandleInputLeaderboard();
                        break;
                    case GameState.Help:
                        HandleInputHelp();
                        break;
                    case GameState.Settings:
                        HandleInputSettings();
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
                // В паузе пропускаем UpdatePlaying, ограничиваемся обработкой паузы
                if (_state == GameState.Paused)
                {
                    UpdatePaused();
                    return;
                }

                switch (_state)
                {
                    case GameState.MainMenu:
                        UpdateMainMenu();
                        break;
                    case GameState.Help:
                        UpdateHelp();
                        break;
                    case GameState.Settings:
                        UpdateSettings();
                        break;
                    case GameState.Playing:
                        UpdatePlaying();
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
                    case GameState.Leaderboard:
                        RenderLeaderboard();
                        break;
                    case GameState.Help:
                        RenderHelp();
                        break;
                    case GameState.Settings:
                        RenderSettings();
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

                // Навигация по пунктам меню с циклическим переходом
                if (key == ConsoleKey.UpArrow)
                {
                    _mainMenuSelectedIndex = (_mainMenuSelectedIndex - 1 + _mainMenuItems.Length) % _mainMenuItems.Length;
                    _mainMenuDrawn = false;
                    return;
                }

                if (key == ConsoleKey.DownArrow)
                {
                    _mainMenuSelectedIndex = (_mainMenuSelectedIndex + 1) % _mainMenuItems.Length;
                    _mainMenuDrawn = false;
                    return;
                }

                if (key == ConsoleKey.Enter)
                {
                    switch (_mainMenuSelectedIndex)
                    {
                        case 0:
                            ResetGame();
                            _state = GameState.Playing;
                            UiFrameRenderer.DrawStaticFrame(board.Width, board.Height); // Рисуем статический каркас при входе в игру
                            break;
                        case 1:
                            _state = GameState.Leaderboard;
                            _leaderboardDrawn = false;
                            break;
                        case 2:
                            _state = GameState.Help;
                            _helpDrawn = false;
                            break;
                        case 3:
                            _state = GameState.Settings;
                            _settingsDrawn = false;
                            _settingsSelectedIndex = (int)_difficulty; // Подсветка текущей сложности при входе
                            break;
                        case 4:
                            Console.Clear();
                            Console.ResetColor();
                            Console.CursorVisible = true;
                            Environment.Exit(0);
                            break;
                    }

                    _mainMenuDrawn = false; // Сбрасываем флаг, чтобы меню перерисовалось при возвращении
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
                    case ConsoleKey.P:
                        // По P ставим игру на паузу и разрешаем отрисовать оверлей
                        _state = GameState.Paused;
                        _pausedDrawn = false;
                        break;
                    case ConsoleKey.B:
                        UsePowerUp();
                        break;
                }
            }

            // Использование текущего бонуса
            private void UsePowerUp()
            {
                if (_currentPowerUp == PowerUpType.None) return;

                if (_currentPowerUp == PowerUpType.Bomb)
                {
                    // Удаляем три нижние строки
                    for (int i = 0; i < 3; i++)
                    {
                        for (int x = 0; x < board.Width; x++)
                        {
                            board.Grid[board.Height - 1 - i, x] = 0;
                        }
                    }
                    // После очистки нижних трех строк сдвигаем всё вниз
                    for (int y = board.Height - 4; y >= 0; y--)
                    {
                        for (int x = 0; x < board.Width; x++)
                        {
                            board.Grid[y + 3, x] = board.Grid[y, x];
                        }
                    }
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < board.Width; x++)
                        {
                            board.Grid[y, x] = 0;
                        }
                    }

                    _score += 150; // Бонус за использование бомбы
                }
                else if (_currentPowerUp == PowerUpType.Freeze)
                {
                    // Заморозка: даем 100 тиков (около 3 секунд) медленного падения
                    _freezeTicksRemaining = 100;
                }

                _currentPowerUp = PowerUpType.None; // Бонус потрачен
            }

            // Ввод в паузе: продолжить игру или выйти в меню
            private void HandleInputPaused()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                ConsoleKey key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.P:
                        // Возвращаемся к игре без очистки поля
                        _state = GameState.Playing;
                        _pausedDrawn = false; // Чтобы следующий вход в паузу нарисовал оверлей заново
                        break;
                    case ConsoleKey.Escape:
                        // Esc сбрасывает игру и возвращает в главное меню
                        ResetGame();
                        _state = GameState.MainMenu;
                        _mainMenuDrawn = false;
                        _pausedDrawn = false;
                        break;
                }
            }

            // Ввод на экране помощи: Esc/Enter возвращают в меню
            private void HandleInputHelp()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter || key == ConsoleKey.Escape || key == ConsoleKey.Spacebar)
                {
                    _state = GameState.MainMenu;
                    _mainMenuDrawn = false;
                    _helpDrawn = false;
                }
            }

            // Ввод на экране настроек: выбор сложности и выход в меню
            private void HandleInputSettings()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    _settingsSelectedIndex = (_settingsSelectedIndex - 1 + _difficultyOptions.Length) % _difficultyOptions.Length;
                    _settingsDrawn = false;
                    return;
                }

                if (key == ConsoleKey.DownArrow)
                {
                    _settingsSelectedIndex = (_settingsSelectedIndex + 1) % _difficultyOptions.Length;
                    _settingsDrawn = false;
                    return;
                }

                if (key == ConsoleKey.Enter)
                {
                    _difficulty = (Difficulty)_settingsSelectedIndex;
                    _dropInterval = GetDropInterval(_difficulty);
                    _state = GameState.MainMenu;
                    _mainMenuDrawn = false;
                    _settingsDrawn = false;
                    return;
                }

                if (key == ConsoleKey.Escape)
                {
                    _state = GameState.MainMenu;
                    _mainMenuDrawn = false;
                    _settingsDrawn = false;
                }
            }

            // Ввод после завершения игры: любой ввод возвращает в меню
            private void HandleInputGameOver()
            {
                if (!Console.KeyAvailable)
                {
                    return;
                }

                Console.ReadKey(true);
                SaveHighScore(_score);
                _state = GameState.MainMenu;
                _mainMenuDrawn = false; // При возвращении в меню разрешаем перерисовать экран один раз
            }

            private void HandleInputLeaderboard()
            {
                if (!Console.KeyAvailable) return;
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter || key == ConsoleKey.Escape || key == ConsoleKey.Spacebar)
                {
                    _state = GameState.MainMenu;
                    _mainMenuDrawn = false;
                    _leaderboardDrawn = false;
                }
            }

            // Обновление в главном меню пока не требуется
            private void UpdateMainMenu()
            {
                // Логика обновления меню не нужна, оставляем заглушку
            }

            // Экран помощи статичен, динамики не требуется
            private void UpdateHelp()
            {
                // Заглушка для возможных будущих анимаций помощи
            }

            // Экран настроек статичен, динамики не требуется
            private void UpdateSettings()
            {
                // Заглушка для будущих настроек
            }

            // Обновление игрового процесса: падение фигур и проверка завершения
            private void UpdatePlaying()
            {
                _tick++; // Увеличиваем счётчик кадров

                int currentInterval = _dropInterval;
                if (_freezeTicksRemaining > 0)
                {
                    currentInterval = _dropInterval * 3; // Замедляем падение в 3 раза
                    _freezeTicksRemaining--;
                }

                bool shouldFall = _tick % currentInterval == 0; // Проверяем, пора ли падать
                if (!shouldFall) return; // Если ещё рано, выходим

                if (!MoveTetrominoDown())
                {
                    board.MergeTetromino(currentTetromino);
                    int clearedLines = board.ClearLines();
                    if (clearedLines > 0)
                    {
                        _linesCleared += clearedLines;
                        _score += clearedLines * 100; // Начисляем очки за линии

                        // Логика получения бонуса: если убрано 4 линии за раз (Тетрис) - даем бонус
                        if (clearedLines >= 4 && _currentPowerUp == PowerUpType.None)
                        {
                            _currentPowerUp = random.Next(2) == 0 ? PowerUpType.Bomb : PowerUpType.Freeze;
                        }
                    }

                    if (!SpawnNewTetromino())
                    {
                        _state = GameState.GameOver; // Переход в Game Over при невозможности спавна
                        _gameOverDrawn = false; // Сбрасываем флаг, чтобы экран нарисовался при входе
                    }
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
                // Меню рисуем только при изменении выделения или входе в состояние
                if (_mainMenuDrawn)
                    return;

                Console.Clear();
                Console.ResetColor();

                int boxWidth = 50;
                int boxHeight = 12;
                int boxX = UiLayout.LeftPanelX;
                int boxY = UiLayout.LeftPanelY;

                UiFrameRenderer.DrawBox(boxX, boxY, boxWidth, boxHeight, " MAIN MENU ");

                // Заголовок меню — просто шире рамка, остальное без изменений
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.LabelColor;
                Console.SetCursorPosition(boxX + 4, boxY + 2);
                Console.Write("=== MaxFunkTetris2024 ===");

                // Пункты меню с подсветкой выбранного
                for (int i = 0; i < _mainMenuItems.Length; i++)
                {
                    Console.SetCursorPosition(boxX + 4, boxY + 4 + i);
                    Console.ForegroundColor = i == _mainMenuSelectedIndex ? UiTheme.ValueColor : UiTheme.LabelColor;
                    Console.Write($"> {_mainMenuItems[i]}");
                }

                // Подсказка по управлению укорочена, чтобы гарантированно помещалась
                Console.ForegroundColor = UiTheme.InfoColor;
                Console.SetCursorPosition(boxX + 4, boxY + boxHeight - 3);
                Console.Write("↑/↓ выбор, Enter — подтвердить");

                Console.ForegroundColor = previousColor;
                _mainMenuDrawn = true;
            }

            // Отрисовка экрана лидерборда
            private void RenderLeaderboard()
            {
                if (_leaderboardDrawn)
                    return;

                Console.Clear();
                Console.ResetColor();

                int boxWidth = 50;
                int boxHeight = 12;
                int boxX = UiLayout.LeftPanelX;
                int boxY = UiLayout.LeftPanelY;

                UiFrameRenderer.DrawBox(boxX, boxY, boxWidth, boxHeight, " LEADERBOARD ");

                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.InfoColor;

                var highScores = LoadHighScores();

                Console.SetCursorPosition(boxX + 3, boxY + 2);
                Console.Write("Топ рекордов:");

                for (int i = 0; i < Math.Min(highScores.Count, 5); i++)
                {
                    Console.SetCursorPosition(boxX + 5, boxY + 4 + i);
                    Console.Write($"{i + 1}. {highScores[i]}");
                }

                if (highScores.Count == 0)
                {
                    Console.SetCursorPosition(boxX + 5, boxY + 4);
                    Console.Write("Рекордов пока нет.");
                }

                Console.ForegroundColor = UiTheme.LabelColor;
                Console.SetCursorPosition(boxX + 3, boxY + boxHeight - 2);
                Console.Write("Enter или Esc — назад.");

                Console.ForegroundColor = previousColor;
                _leaderboardDrawn = true;
            }

            // Отрисовка экрана помощи
            private void RenderHelp()
            {
                if (_helpDrawn)
                    return;

                Console.Clear();
                Console.ResetColor();

                int boxWidth = 50;
                int boxHeight = 10;
                int boxX = UiLayout.LeftPanelX;
                int boxY = UiLayout.LeftPanelY;

                UiFrameRenderer.DrawBox(boxX, boxY, boxWidth, boxHeight, " HELP ");

                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.InfoColor;
                Console.SetCursorPosition(boxX + 3, boxY + 2);
                Console.Write("Управление: ←/→ — движение, ↑ — вращение");
                Console.SetCursorPosition(boxX + 3, boxY + 3);
                Console.Write("↓ — ускорение падения, P — пауза");
                Console.SetCursorPosition(boxX + 3, boxY + 4);
                Console.Write("Esc — вернуться в меню");

                Console.SetCursorPosition(boxX + 3, boxY + 6);
                Console.Write("Цель: заполнять линии, чтобы они исчезали");
                Console.SetCursorPosition(boxX + 3, boxY + 7);
                Console.Write("и приносили очки.");

                Console.SetCursorPosition(boxX + 3, boxY + boxHeight - 2);
                Console.Write("Enter или Esc — вернуться в меню.");

                Console.ForegroundColor = previousColor;
                _helpDrawn = true;
            }

            // Отрисовка экрана настроек
            private void RenderSettings()
            {
                if (_settingsDrawn)
                    return;

                Console.Clear();
                Console.ResetColor();

                int boxWidth = 50;
                int boxHeight = 10;
                int boxX = UiLayout.LeftPanelX;
                int boxY = UiLayout.LeftPanelY;

                UiFrameRenderer.DrawBox(boxX, boxY, boxWidth, boxHeight, " SETTINGS ");

                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.InfoColor;
                Console.SetCursorPosition(boxX + 3, boxY + 2);
                Console.Write("Выберите сложность:");

                for (int i = 0; i < _difficultyOptions.Length; i++)
                {
                    bool isSelected = i == _settingsSelectedIndex;
                    bool isCurrent = (Difficulty)i == _difficulty;
                    Console.SetCursorPosition(boxX + 5, boxY + 4 + i);
                    Console.ForegroundColor = isSelected ? UiTheme.ValueColor : UiTheme.InfoColor;
                    string marker = isCurrent ? "*" : " ";
                    Console.Write($"> [{marker}] {_difficultyOptions[i]}");
                }

                Console.ForegroundColor = UiTheme.LabelColor;
                Console.SetCursorPosition(boxX + 3, boxY + boxHeight - 3);
                Console.Write("↑/↓ — выбор, Enter — сохранить");
                Console.SetCursorPosition(boxX + 3, boxY + boxHeight - 2);
                Console.Write("Esc — назад.");

                Console.ForegroundColor = previousColor;
                _settingsDrawn = true;
            }

            // Отрисовка игрового процесса с полем и очками
            private void RenderPlaying()
            {
                GameRenderer.Render(board, currentTetromino);
                UiStatsRenderer.RenderStats(_score, CurrentLevel, _linesCleared);
                UiStatsRenderer.RenderNextTetromino(nextTetromino, board.Width);
                UiStatsRenderer.RenderInventory(_currentPowerUp, board.Width);
            }

#if DEBUG
            // Рисуем небольшую строку профилировки кадра под правой панелью, чтобы не мешать игровому полю
            private void RenderFrameDiagnostics()
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.InfoColor;

                int infoX = UiLayout.GetInfoPanelX(board.Width);
                int infoY = UiLayout.InfoPanelY + UiLayout.RightPanelHeight + 1;
                Console.SetCursorPosition(infoX, infoY);
                Console.Write($"Frame: {_lastFrameMilliseconds,6:0.0} ms max {_maxFrameMilliseconds,6:0.0} ms   ");

                Console.ForegroundColor = previousColor;
            }
#endif

            // Отрисовка паузы поверх игрового поля без очистки кадра
            private void RenderPaused()
            {
                if (_pausedDrawn)
                    return;

                // Рисуем компактный бокс по центру поля, который гарантированно помещается внутрь рамки
                int overlayWidth = Math.Min(_width, Math.Max(14, Math.Min(_width, 18)));
                int overlayHeight = 8;
                int overlayX = UiLayout.BoardOffsetX + (_width - overlayWidth) / 2;
                int overlayY = UiLayout.BoardOffsetY + (_height - overlayHeight) / 2;

                UiFrameRenderer.DrawBox(overlayX, overlayY, overlayWidth, overlayHeight, " PAUSED ");

                // Затираем фон внутри рамки, чтобы не просвечивали точки поля или текст панели
                for (int y = overlayY + 1; y < overlayY + overlayHeight - 1; y++)
                {
                    Console.SetCursorPosition(overlayX + 1, y);
                    Console.Write(new string(' ', overlayWidth - 2));
                }

                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = UiTheme.InfoColor;

                Console.SetCursorPosition(overlayX + 2, overlayY + 2);
                Console.Write("Игра на паузе");
                Console.SetCursorPosition(overlayX + 2, overlayY + 4);
                Console.Write("P — продолж.");
                Console.SetCursorPosition(overlayX + 2, overlayY + 5);
                Console.Write("Esc — меню");

                Console.ForegroundColor = previousColor;
                _pausedDrawn = true;
            }

            // Отрисовка экрана завершения игры
            private void RenderGameOver()
            {
                // Экран Game Over рисуем один раз при входе, чтобы исключить мерцание
                if (_gameOverDrawn)
                    return;

                Console.Clear();
                Console.ResetColor();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Game Over");
                Console.WriteLine($"Итоговый счёт: {_score}");
                Console.WriteLine();
                Console.WriteLine("Нажмите любую клавишу, чтобы вернуться в главное меню.");

                _gameOverDrawn = true;
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
                // Сохраняем форму и координаты перед попыткой вращения
                var savedState = currentTetromino.SaveState();

                currentTetromino.Rotate();
                if (board.IsCollision(currentTetromino))
                {
                    // Возвращаем сохранённое состояние вместо повторного вращения
                    currentTetromino.RestoreState(savedState.shape, savedState.x, savedState.y);
                }
            }

            // Метод для создания нового тетромино
            private bool SpawnNewTetromino()
            {
                currentTetromino = nextTetromino;
                nextTetromino = CreateNewTetromino();
                return !board.IsCollision(currentTetromino);
            }

            // Загрузка рекордов
            private System.Collections.Generic.List<int> LoadHighScores()
            {
                var scores = new System.Collections.Generic.List<int>();
                if (System.IO.File.Exists(HighscoreFilePath))
                {
                    try
                    {
                        var lines = System.IO.File.ReadAllLines(HighscoreFilePath);
                        foreach (var line in lines)
                        {
                            if (int.TryParse(line, out int score))
                            {
                                scores.Add(score);
                            }
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки чтения файла
                    }
                }
                scores.Sort((a, b) => b.CompareTo(a));
                return scores;
            }

            // Сохранение рекорда
            private void SaveHighScore(int score)
            {
                if (score <= 0) return;
                var scores = LoadHighScores();
                scores.Add(score);
                scores.Sort((a, b) => b.CompareTo(a));
                try
                {
                    System.IO.File.WriteAllLines(HighscoreFilePath, System.Linq.Enumerable.Select(scores.Take(5), s => s.ToString()));
                }
                catch
                {
                    // Игнорируем ошибки записи
                }
            }
        }

    }
}