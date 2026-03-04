using System;
using System.Diagnostics;
using System.Linq;

namespace MaxFunkTetris2024
{
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

        private GameSettings _settings; // Текущие настройки игры
        private GameMode _mode; // Текущий режим игры
        private Difficulty _difficulty; // Текущая сложность игры (для совместимости)

        private PowerUpType _currentPowerUp; // Текущий бонус в инвентаре
        private int _freezeTicksRemaining; // Оставшееся время заморозки

        private int _settingsMenuIndex; // 0 - Mode, 1 - Difficulty, 2 - Skin, 3 - Keys
        private int _settingsKeysIndex; // Индекс в меню кнопок
        private bool _inSettingsKeysMode; // Режим назначения кнопки

        private readonly string[] _difficultyOptions = { "Easy", "Normal", "Hard" }; // Подписи пунктов сложности
        private readonly string[] _modeOptions = { "Classic", "Marathon" }; // Подписи режимов
        private readonly char[] _skinOptions = { '█', '▓', '▒', '░', '#' };

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
            _settings = GameSettings.Load();
            _state = GameState.MainMenu; // Запуск с главного меню
            _difficulty = _settings.Difficulty; // Базовая сложность из настроек
            _mode = _settings.Mode;
            _settingsSelectedIndex = 0;
            _settingsMenuIndex = 0;
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
            int baseInterval = difficulty switch
            {
                Difficulty.Easy => 10,
                Difficulty.Normal => 6,
                Difficulty.Hard => 3,
                _ => 6
            };

            if (_mode == GameMode.Marathon)
            {
                // В марафоне скорость увеличивается с уровнем
                int decrease = (CurrentLevel - 1);
                baseInterval -= decrease;
                if (baseInterval < 1) baseInterval = 1;
            }

            return baseInterval;
        }

        // Фабрика для создания случайного тетрамино
        private Tetromino CreateNewTetromino()
        {
            return new Tetromino(random.Next(7));
        }

        // Основной игровой цикл со стейт-машиной
        // Свойство для выхода из цикла в тестах
        public bool IsRunning { get; set; } = true;

        public void Run()
        {
            const int frameDelayMs = 30; // ~33 FPS фиксированная задержка кадра
            // Прячем курсор один раз перед стартом игрового цикла, чтобы не мешал анимации
            try { Console.CursorVisible = false; } catch (System.IO.IOException) { }

            while (IsRunning)
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

            if (key == _settings.LeftKey)
            {
                MoveTetromino(-1, 0);
            }
            else if (key == _settings.RightKey)
            {
                MoveTetromino(1, 0);
            }
            else if (key == _settings.DownKey)
            {
                MoveTetrominoDown();
            }
            else if (key == _settings.RotateKey)
            {
                RotateTetromino();
            }
            else if (key == _settings.PauseKey)
            {
                _state = GameState.Paused;
                _pausedDrawn = false;
            }
            else if (key == _settings.PowerUpKey)
            {
                UsePowerUp();
            }
        }

        // Использование текущего бонуса
        private void UsePowerUp()
        {
            if (_currentPowerUp == PowerUpType.None) return;

            if (_currentPowerUp == PowerUpType.Bomb)
            {
                // Удаляем три нижние строки (или меньше, если поле маленькое)
                int linesToRemove = Math.Min(3, board.Height);
                for (int i = 0; i < linesToRemove; i++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        board.Grid[board.Height - 1 - i, x] = 0;
                    }
                }
                // После очистки нижних трех строк сдвигаем всё вниз
                for (int y = board.Height - 1 - linesToRemove; y >= 0; y--)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        board.Grid[y + linesToRemove, x] = board.Grid[y, x];
                    }
                }
                for (int y = 0; y < linesToRemove; y++)
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

            if (_inSettingsKeysMode)
            {
                // Если мы в режиме назначения кнопки, перехватываем любую кнопку кроме Escape
                if (key == ConsoleKey.Escape)
                {
                    _inSettingsKeysMode = false;
                    _settingsDrawn = false;
                    return;
                }

                switch (_settingsKeysIndex)
                {
                    case 0: _settings.LeftKey = key; break;
                    case 1: _settings.RightKey = key; break;
                    case 2: _settings.DownKey = key; break;
                    case 3: _settings.RotateKey = key; break;
                    case 4: _settings.PauseKey = key; break;
                    case 5: _settings.PowerUpKey = key; break;
                }

                _inSettingsKeysMode = false;
                _settingsDrawn = false;
                _settings.Save();
                return;
            }

            if (key == ConsoleKey.UpArrow)
            {
                if (_settingsMenuIndex == 3) // Управление
                {
                    _settingsKeysIndex = (_settingsKeysIndex - 1 + 6) % 6;
                }
                else
                {
                    _settingsMenuIndex = (_settingsMenuIndex - 1 + 4) % 4;
                }
                _settingsDrawn = false;
                return;
            }

            if (key == ConsoleKey.DownArrow)
            {
                if (_settingsMenuIndex == 3) // Управление
                {
                    _settingsKeysIndex = (_settingsKeysIndex + 1) % 6;
                }
                else
                {
                    _settingsMenuIndex = (_settingsMenuIndex + 1) % 4;
                }
                _settingsDrawn = false;
                return;
            }

            if (key == ConsoleKey.LeftArrow)
            {
                if (_settingsMenuIndex == 0)
                {
                    _mode = (GameMode)(((int)_mode - 1 + _modeOptions.Length) % _modeOptions.Length);
                }
                else if (_settingsMenuIndex == 1)
                {
                    _difficulty = (Difficulty)(((int)_difficulty - 1 + _difficultyOptions.Length) % _difficultyOptions.Length);
                }
                else if (_settingsMenuIndex == 2)
                {
                    int currentSkinIndex = Array.IndexOf(_skinOptions, _settings.BlockChar);
                    currentSkinIndex = (currentSkinIndex - 1 + _skinOptions.Length) % _skinOptions.Length;
                    _settings.BlockChar = _skinOptions[currentSkinIndex];
                }
                _settingsDrawn = false;
                return;
            }

            if (key == ConsoleKey.RightArrow)
            {
                if (_settingsMenuIndex == 0)
                {
                    _mode = (GameMode)(((int)_mode + 1) % _modeOptions.Length);
                }
                else if (_settingsMenuIndex == 1)
                {
                    _difficulty = (Difficulty)(((int)_difficulty + 1) % _difficultyOptions.Length);
                }
                else if (_settingsMenuIndex == 2)
                {
                    int currentSkinIndex = Array.IndexOf(_skinOptions, _settings.BlockChar);
                    currentSkinIndex = (currentSkinIndex + 1) % _skinOptions.Length;
                    _settings.BlockChar = _skinOptions[currentSkinIndex];
                }
                _settingsDrawn = false;
                return;
            }

            if (key == ConsoleKey.Enter)
            {
                if (_settingsMenuIndex == 3)
                {
                    _inSettingsKeysMode = true;
                    _settingsDrawn = false;
                }
                else
                {
                    _settings.Difficulty = _difficulty;
                    _settings.Mode = _mode;
                    _settings.Save();
                    _dropInterval = GetDropInterval(_difficulty);
                    _state = GameState.MainMenu;
                    _mainMenuDrawn = false;
                    _settingsDrawn = false;
                }
                return;
            }

            if (key == ConsoleKey.Escape)
            {
                if (_settingsMenuIndex == 3)
                {
                    _settingsMenuIndex = 0; // Возврат к основному меню настроек
                    _settingsDrawn = false;
                }
                else
                {
                    _settings.Difficulty = _difficulty;
                    _settings.Mode = _mode;
                    _settings.Save();
                    _state = GameState.MainMenu;
                    _mainMenuDrawn = false;
                    _settingsDrawn = false;
                }
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

                var linesToClear = board.GetLinesToClear();
                if (linesToClear.Count > 0)
                {
                    // Анимация: мигание заполненных линий (2 раза)
                    try
                    {
                        for (int flash = 0; flash < 2; flash++)
                        {
                            // Закрашиваем линию спецсимволом (например, '*')
                            foreach (var y in linesToClear)
                            {
                                Console.SetCursorPosition(UiLayout.BoardOffsetX, UiLayout.BoardOffsetY + y);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(new string('*', board.Width));
                            }
                            System.Threading.Thread.Sleep(80);

                            // Закрашиваем пустотой (пробелами)
                            foreach (var y in linesToClear)
                            {
                                Console.SetCursorPosition(UiLayout.BoardOffsetX, UiLayout.BoardOffsetY + y);
                                Console.Write(new string(' ', board.Width));
                            }
                            System.Threading.Thread.Sleep(80);
                        }
                        Console.ResetColor();
                    }
                    catch (System.IO.IOException) { } // Игнорируем ошибки консоли в тестах

                    board.ClearSpecificLines(linesToClear);

                    int clearedLines = linesToClear.Count;
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
            int boxHeight = 18;
            int boxX = UiLayout.LeftPanelX;
            int boxY = UiLayout.LeftPanelY;

            UiFrameRenderer.DrawBox(boxX, boxY, boxWidth, boxHeight, " SETTINGS ");

            ConsoleColor previousColor = Console.ForegroundColor;

            // Mode
            Console.SetCursorPosition(boxX + 3, boxY + 2);
            Console.ForegroundColor = _settingsMenuIndex == 0 ? UiTheme.ValueColor : UiTheme.InfoColor;
            Console.Write($"{( _settingsMenuIndex == 0 ? ">" : " ")} Режим: < {_modeOptions[(int)_mode]} >");

            // Difficulty
            Console.SetCursorPosition(boxX + 3, boxY + 3);
            Console.ForegroundColor = _settingsMenuIndex == 1 ? UiTheme.ValueColor : UiTheme.InfoColor;
            Console.Write($"{( _settingsMenuIndex == 1 ? ">" : " ")} Сложность: < {_difficultyOptions[(int)_difficulty]} >");

            // Skin
            Console.SetCursorPosition(boxX + 3, boxY + 4);
            Console.ForegroundColor = _settingsMenuIndex == 2 ? UiTheme.ValueColor : UiTheme.InfoColor;
            Console.Write($"{( _settingsMenuIndex == 2 ? ">" : " ")} Скин блока: < {_settings.BlockChar} >");

            // Keys
            Console.SetCursorPosition(boxX + 3, boxY + 6);
            Console.ForegroundColor = _settingsMenuIndex == 3 ? UiTheme.ValueColor : UiTheme.InfoColor;
            Console.Write($"{( _settingsMenuIndex == 3 ? ">" : " ")} Управление:");

            string[] keyNames = { "Влево", "Вправо", "Вниз", "Вращение", "Пауза", "Бонус" };
            ConsoleKey[] keyValues = { _settings.LeftKey, _settings.RightKey, _settings.DownKey, _settings.RotateKey, _settings.PauseKey, _settings.PowerUpKey };

            for (int i = 0; i < keyNames.Length; i++)
            {
                Console.SetCursorPosition(boxX + 5, boxY + 8 + i);

                if (_settingsMenuIndex == 3 && _settingsKeysIndex == i)
                {
                    Console.ForegroundColor = _inSettingsKeysMode ? ConsoleColor.Yellow : UiTheme.ValueColor;
                    string val = _inSettingsKeysMode ? "[ Нажмите кнопку... ]" : keyValues[i].ToString();
                    Console.Write($"> {keyNames[i],-10}: {val}");
                }
                else
                {
                    Console.ForegroundColor = UiTheme.InfoColor;
                    Console.Write($"  {keyNames[i],-10}: {keyValues[i]}");
                }
            }

            Console.ForegroundColor = UiTheme.LabelColor;
            Console.SetCursorPosition(boxX + 3, boxY + boxHeight - 3);
            Console.Write("↑/↓/←/→ — выбор/изменение, Enter — применить");
            Console.SetCursorPosition(boxX + 3, boxY + boxHeight - 2);
            Console.Write("Esc — назад.");

            Console.ForegroundColor = previousColor;
            _settingsDrawn = true;
        }

        // Отрисовка игрового процесса с полем и очками
        private void RenderPlaying()
        {
            GameRenderer.Render(board, currentTetromino, _settings.BlockChar);
            UiStatsRenderer.RenderStats(_score, CurrentLevel, _linesCleared);
            UiStatsRenderer.RenderNextTetromino(nextTetromino, board.Width, _settings.BlockChar);
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
