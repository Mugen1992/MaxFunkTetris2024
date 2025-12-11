using System.Reflection;
using MaxFunkTetris2024;
using Xunit;

namespace MaxFunkTetris2024.Tests
{
    // Минимальные интеграционные проверки стейт-машины без настоящей консоли
    public class GameStateMachineTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void InitialState_Is_MainMenu()
        {
            var game = new Program.Game(15, 20);
            var stateField = typeof(Program.Game).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(stateField);

            Assert.Equal(GameState.MainMenu, stateField!.GetValue(game));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UpdatePlaying_WithBlockedSpawn_SetsGameOver()
        {
            var game = new Program.Game(10, 4);
            var boardField = typeof(Program.Game).GetField("board", BindingFlags.NonPublic | BindingFlags.Instance);
            var tetrominoField = typeof(Program.Game).GetField("currentTetromino", BindingFlags.NonPublic | BindingFlags.Instance);
            var tickField = typeof(Program.Game).GetField("_tick", BindingFlags.NonPublic | BindingFlags.Instance);
            var dropIntervalField = typeof(Program.Game).GetField("_dropInterval", BindingFlags.NonPublic | BindingFlags.Instance);
            var stateField = typeof(Program.Game).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(boardField);
            Assert.NotNull(tetrominoField);
            Assert.NotNull(tickField);
            Assert.NotNull(dropIntervalField);
            Assert.NotNull(stateField);

            var board = new Program.GameBoard(10, 4);
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.Grid[y, x] = x == board.Width - 1 ? 0 : 1;
                }
            }

            var tetromino = new Program.Tetromino(0);
            tetromino.X = 4;
            tetromino.Y = 0;

            boardField!.SetValue(game, board);
            tetrominoField!.SetValue(game, tetromino);
            dropIntervalField!.SetValue(game, 1);
            tickField!.SetValue(game, 0);

            MethodInfo? updatePlaying = typeof(Program.Game).GetMethod("UpdatePlaying", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(updatePlaying);

            updatePlaying!.Invoke(game, System.Array.Empty<object>());

            Assert.Equal(GameState.GameOver, (GameState)stateField!.GetValue(game)!);
        }
    }
}
