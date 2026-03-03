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
            var game = new Game(15, 20);
            var stateField = typeof(Game).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(stateField);

            Assert.Equal(GameState.MainMenu, stateField!.GetValue(game));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void UpdatePlaying_WithBlockedSpawn_SetsGameOver()
        {
            var game = new Game(10, 4);
            var boardField = typeof(Game).GetField("board", BindingFlags.NonPublic | BindingFlags.Instance);
            var tetrominoField = typeof(Game).GetField("currentTetromino", BindingFlags.NonPublic | BindingFlags.Instance);
            var tickField = typeof(Game).GetField("_tick", BindingFlags.NonPublic | BindingFlags.Instance);
            var dropIntervalField = typeof(Game).GetField("_dropInterval", BindingFlags.NonPublic | BindingFlags.Instance);
            var stateField = typeof(Game).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(boardField);
            Assert.NotNull(tetrominoField);
            Assert.NotNull(tickField);
            Assert.NotNull(dropIntervalField);
            Assert.NotNull(stateField);

            var board = new GameBoard(10, 4);
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.Grid[y, x] = x == board.Width - 1 ? 0 : 1;
                }
            }

            var tetromino = new Tetromino(0);
            tetromino.X = 4;
            tetromino.Y = 0;

            boardField!.SetValue(game, board);
            tetrominoField!.SetValue(game, tetromino);
            dropIntervalField!.SetValue(game, 1);
            tickField!.SetValue(game, 0);

            MethodInfo? updatePlaying = typeof(Game).GetMethod("UpdatePlaying", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(updatePlaying);

            updatePlaying!.Invoke(game, System.Array.Empty<object>());

            Assert.Equal(GameState.GameOver, (GameState)stateField!.GetValue(game)!);
        }
    }
}
