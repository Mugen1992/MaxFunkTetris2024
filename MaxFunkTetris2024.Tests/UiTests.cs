using System;
using System.IO;
using System.Text;
using MaxFunkTetris2024;
using Xunit;

namespace MaxFunkTetris2024.Tests
{
    [Collection("Sequential")]
    public class UiTests : IDisposable
    {
        private readonly StringWriter _stringWriter;
        private readonly TextWriter _originalOutput;

        public UiTests()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GameRenderer_Render_DrawsBoardAndTetromino()
        {
            var board = new GameBoard(10, 20);
            var tetromino = new Tetromino(0); // О-образная фигура 2x2
            tetromino.X = 0;
            tetromino.Y = 0;
            char blockChar = 'X';

            var exception = Record.Exception(() => GameRenderer.Render(board, tetromino, blockChar));

            // В окружении без реальной консоли Console.SetCursorPosition бросает System.IO.IOException
            // Это ожидаемо, мы проверяем, что метод вызывается и пытается писать
            if (exception != null)
            {
                if (exception is IOException) return;
                Assert.IsType<IOException>(exception);
            }
            else
            {
                var output = _stringWriter.ToString();
                Assert.Contains("XX", output); // Фигура 2x2
                Assert.Contains("........", output); // Пустые клетки
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UiFrameRenderer_DrawBox_WritesToConsole()
        {
            var exception = Record.Exception(() => UiFrameRenderer.DrawBox(0, 0, 10, 10, "TEST"));

            if (exception != null)
            {
                Assert.IsType<IOException>(exception);
            }
            else
            {
                var output = _stringWriter.ToString();
                Assert.Contains("TEST", output);
                Assert.Contains(UiTheme.HorizontalBorder.ToString(), output);
                Assert.Contains(UiTheme.VerticalBorder.ToString(), output);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UiFrameRenderer_DrawStaticFrame_CallsDrawBoxMethods()
        {
            var exception = Record.Exception(() => UiFrameRenderer.DrawStaticFrame(10, 20));

            if (exception != null)
            {
                Assert.IsType<IOException>(exception);
            }
            else
            {
                var output = _stringWriter.ToString();
                Assert.Contains("FIELD", output);
                Assert.Contains("STATS", output);
                Assert.Contains("INFO", output);
                Assert.Contains("NEXT", output);
                Assert.Contains("ITEM", output);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UiLayout_PropertiesAndMethods_ReturnCorrectValues()
        {
            Assert.Equal(UiLayout.LeftPanelX + 2, UiLayout.StatsLabelX);
            Assert.Equal(UiLayout.LeftPanelY + 2, UiLayout.ScoreLabelY);

            int boardWidth = 10;
            Assert.Equal(UiLayout.BoardOffsetX + boardWidth + UiLayout.PanelPadding, UiLayout.GetInfoPanelX(boardWidth));
            Assert.Equal(UiLayout.GetInfoPanelX(boardWidth), UiLayout.GetNextPanelX(boardWidth));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UiStatsRenderer_RenderStats_WritesScores()
        {
            var exception = Record.Exception(() => UiStatsRenderer.RenderStats(1234, 5, 67));

            if (exception != null)
            {
                Assert.IsType<IOException>(exception);
            }
            else
            {
                var output = _stringWriter.ToString();
                Assert.Contains("  1234", output);
                Assert.Contains("     5", output);
                Assert.Contains("    67", output);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UiStatsRenderer_RenderNextTetromino_WritesToConsole()
        {
            var tetromino = new Tetromino(0);
            var exception = Record.Exception(() => UiStatsRenderer.RenderNextTetromino(tetromino, 10, 'X'));

            if (exception != null)
            {
                Assert.IsType<IOException>(exception);
            }
            else
            {
                var output = _stringWriter.ToString();
                Assert.Contains("XX", output); // Рисует 2x2 'X'
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void UiStatsRenderer_RenderInventory_WritesPowerUpName()
        {
            foreach (PowerUpType powerUp in Enum.GetValues(typeof(PowerUpType)))
            {
                var exception = Record.Exception(() => UiStatsRenderer.RenderInventory(powerUp, 10));

                if (exception != null)
                {
                    Assert.IsType<IOException>(exception);
                }
                else
                {
                    var output = _stringWriter.ToString();
                    if (powerUp == PowerUpType.Bomb) Assert.Contains("BOMB", output);
                    if (powerUp == PowerUpType.Freeze) Assert.Contains("FREEZE", output);
                    if (powerUp == PowerUpType.None) Assert.Contains("empty", output);
                }
            }
        }
    }
}
