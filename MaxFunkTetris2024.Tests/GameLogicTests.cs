using System;
using System.Reflection;
using MaxFunkTetris2024;
using Xunit;

namespace MaxFunkTetris2024.Tests
{
    // Юнит-тесты логики игры без запуска консольного цикла
    public class GameLogicTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void GetDropInterval_ReturnsExpectedValues_ForEachDifficulty()
        {
            var game = new Program.Game(10, 20);
            MethodInfo? method = typeof(Program.Game).GetMethod("GetDropInterval", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            Assert.Equal(10, method!.Invoke(game, new object[] { Difficulty.Easy }));
            Assert.Equal(6, method.Invoke(game, new object[] { Difficulty.Normal }));
            Assert.Equal(3, method.Invoke(game, new object[] { Difficulty.Hard }));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ResetGame_SetsScoreLinesAndTickToZero()
        {
            var game = new Program.Game(10, 20);
            MethodInfo? resetMethod = typeof(Program.Game).GetMethod("ResetGame", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(resetMethod);

            SetPrivateField(game, "_score", 500);
            SetPrivateField(game, "_linesCleared", 7);
            SetPrivateField(game, "_tick", 12);

            resetMethod!.Invoke(game, Array.Empty<object>());

            Assert.Equal(0, GetPrivateField<int>(game, "_score"));
            Assert.Equal(0, GetPrivateField<int>(game, "_linesCleared"));
            Assert.Equal(0, GetPrivateField<int>(game, "_tick"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UpdatePlaying_WhenLineCleared_IncreasesScoreAndLines()
        {
            var game = new Program.Game(10, 4);
            var boardField = typeof(Program.Game).GetField("board", BindingFlags.NonPublic | BindingFlags.Instance);
            var tetrominoField = typeof(Program.Game).GetField("currentTetromino", BindingFlags.NonPublic | BindingFlags.Instance);
            var tickField = typeof(Program.Game).GetField("_tick", BindingFlags.NonPublic | BindingFlags.Instance);
            var dropIntervalField = typeof(Program.Game).GetField("_dropInterval", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(boardField);
            Assert.NotNull(tetrominoField);
            Assert.NotNull(tickField);
            Assert.NotNull(dropIntervalField);

            var board = new Program.GameBoard(10, 4);
            // Заполняем нижнюю строку почти полностью, оставляя два правых столбца пустыми
            for (int x = 0; x < board.Width - 2; x++)
            {
                board.Grid[board.Height - 1, x] = 1;
            }

            var tetromino = new Program.Tetromino(0);
            tetromino.X = board.Width - tetromino.Shape.GetLength(1);
            tetromino.Y = board.Height - tetromino.Shape.GetLength(0);

            boardField!.SetValue(game, board);
            tetrominoField!.SetValue(game, tetromino);
            dropIntervalField!.SetValue(game, 1); // заставляем падать каждый тик
            tickField!.SetValue(game, 0);

            MethodInfo? updatePlaying = typeof(Program.Game).GetMethod("UpdatePlaying", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(updatePlaying);

            updatePlaying!.Invoke(game, Array.Empty<object>());

            Assert.Equal(100, GetPrivateField<int>(game, "_score"));
            Assert.Equal(1, GetPrivateField<int>(game, "_linesCleared"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UsePowerUp_Bomb_OnSmallBoard_DoesNotThrow()
        {
            var game = new Program.Game(10, 2); // Очень маленькое поле
            var boardField = typeof(Program.Game).GetField("board", BindingFlags.NonPublic | BindingFlags.Instance);
            var powerUpField = typeof(Program.Game).GetField("_currentPowerUp", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(boardField);
            Assert.NotNull(powerUpField);

            var board = new Program.GameBoard(10, 2);
            boardField!.SetValue(game, board);
            powerUpField!.SetValue(game, PowerUpType.Bomb);

            MethodInfo? usePowerUp = typeof(Program.Game).GetMethod("UsePowerUp", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(usePowerUp);

            // Не должно вызывать исключений
            var exception = Record.Exception(() => usePowerUp!.Invoke(game, Array.Empty<object>()));
            Assert.Null(exception);

            // Бонус должен быть потрачен
            Assert.Equal(PowerUpType.None, GetPrivateField<PowerUpType>(game, "_currentPowerUp"));
            // Score должен увеличиться на 150
            Assert.Equal(150, GetPrivateField<int>(game, "_score"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UsePowerUp_Freeze_AppliesSlowdown()
        {
            var game = new Program.Game(10, 20);
            var powerUpField = typeof(Program.Game).GetField("_currentPowerUp", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(powerUpField);
            powerUpField!.SetValue(game, PowerUpType.Freeze);

            MethodInfo? usePowerUp = typeof(Program.Game).GetMethod("UsePowerUp", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(usePowerUp);

            usePowerUp!.Invoke(game, Array.Empty<object>());

            Assert.Equal(PowerUpType.None, GetPrivateField<PowerUpType>(game, "_currentPowerUp"));
            Assert.Equal(100, GetPrivateField<int>(game, "_freezeTicksRemaining"));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UpdatePlaying_WhenFourLinesCleared_GrantsPowerUp()
        {
            var game = new Program.Game(10, 4);
            var boardField = typeof(Program.Game).GetField("board", BindingFlags.NonPublic | BindingFlags.Instance);
            var tetrominoField = typeof(Program.Game).GetField("currentTetromino", BindingFlags.NonPublic | BindingFlags.Instance);
            var tickField = typeof(Program.Game).GetField("_tick", BindingFlags.NonPublic | BindingFlags.Instance);
            var dropIntervalField = typeof(Program.Game).GetField("_dropInterval", BindingFlags.NonPublic | BindingFlags.Instance);
            var powerUpField = typeof(Program.Game).GetField("_currentPowerUp", BindingFlags.NonPublic | BindingFlags.Instance);

            var board = new Program.GameBoard(10, 4);
            // Очищаем 4 линии за один раз - для этого нам нужна ситуация, когда фигура 1х4 замыкает 4 линии
            // Заполним все 4 строки, оставив один столбец пустым
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < board.Width - 1; x++)
                {
                    board.Grid[y, x] = 1;
                }
            }

            var tetromino = new Program.Tetromino(1); // I-форма 1x4
            tetromino.Rotate(); // Теперь это 4x1 (вертикальная линия)
            tetromino.X = board.Width - 1; // В последнем столбце
            tetromino.Y = 0; // Начинает сверху

            boardField!.SetValue(game, board);
            tetrominoField!.SetValue(game, tetromino);
            dropIntervalField!.SetValue(game, 1);
            tickField!.SetValue(game, 0);

            MethodInfo? updatePlaying = typeof(Program.Game).GetMethod("UpdatePlaying", BindingFlags.NonPublic | BindingFlags.Instance);

            updatePlaying!.Invoke(game, Array.Empty<object>());

            Assert.Equal(4, GetPrivateField<int>(game, "_linesCleared"));
            // Бонус должен быть выдан
            var powerup = GetPrivateField<PowerUpType>(game, "_currentPowerUp");
            Assert.NotEqual(PowerUpType.None, powerup);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void RotateTetromino_RestoresState_IfCollisionOccurs()
        {
            var game = new Program.Game(10, 20);
            var boardField = typeof(Program.Game).GetField("board", BindingFlags.NonPublic | BindingFlags.Instance);
            var tetrominoField = typeof(Program.Game).GetField("currentTetromino", BindingFlags.NonPublic | BindingFlags.Instance);

            var board = new Program.GameBoard(10, 20);
            var tetromino = new Program.Tetromino(1); // I-форма 1x4
            tetromino.X = 9; // Самый правый столбец
            tetromino.Y = 10;
            // Изначально ширина 4, но если x=9, он уже выходит за пределы, но при спавне это может быть.
            // Сделаем вертикальным, чтобы он помещался
            tetromino.Rotate(); // ширина 1, высота 4, x=9 помещается

            boardField!.SetValue(game, board);
            tetrominoField!.SetValue(game, tetromino);

            MethodInfo? rotateMethod = typeof(Program.Game).GetMethod("RotateTetromino", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(rotateMethod);

            // Вращение сделает его шириной 4 (x=9 до 12) - это коллизия (x >= 10)
            rotateMethod!.Invoke(game, Array.Empty<object>());

            // Состояние должно быть восстановлено
            Assert.Equal(1, GetPrivateField<Program.Tetromino>(game, "currentTetromino").Shape.GetLength(1));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CurrentLevel_Increases_Every10Lines()
        {
            var game = new Program.Game(10, 20);
            SetPrivateField(game, "_linesCleared", 0);

            var currentLevelProp = typeof(Program.Game).GetProperty("CurrentLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(currentLevelProp);

            Assert.Equal(1, currentLevelProp!.GetValue(game));

            SetPrivateField(game, "_linesCleared", 9);
            Assert.Equal(1, currentLevelProp!.GetValue(game));

            SetPrivateField(game, "_linesCleared", 10);
            Assert.Equal(2, currentLevelProp!.GetValue(game));

            SetPrivateField(game, "_linesCleared", 25);
            Assert.Equal(3, currentLevelProp!.GetValue(game));
        }

        private static void SetPrivateField<T>(Program.Game game, string name, T value)
        {
            FieldInfo? field = typeof(Program.Game).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field!.SetValue(game, value);
        }

        private static T GetPrivateField<T>(Program.Game game, string name)
        {
            FieldInfo? field = typeof(Program.Game).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            return (T)field!.GetValue(game)!;
        }
    }
}
