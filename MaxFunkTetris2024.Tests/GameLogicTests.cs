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
