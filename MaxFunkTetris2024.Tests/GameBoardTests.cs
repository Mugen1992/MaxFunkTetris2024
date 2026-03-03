using MaxFunkTetris2024;
using Xunit;

namespace MaxFunkTetris2024.Tests
{
    // Юнит-тесты поведения игрового поля
    public class GameBoardTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void IsCollision_ReturnsFalse_ForEmptyBoardAndCenteredPiece()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(0);
            tetromino.X = 3;
            tetromino.Y = 5;

            Assert.False(board.IsCollision(tetromino));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IsCollision_ReturnsTrue_WhenPieceHitsLeftBorder()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(1);
            tetromino.X = -1;
            tetromino.Y = 0;

            Assert.True(board.IsCollision(tetromino));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IsCollision_ReturnsFalse_WhenPieceIsPartiallyAboveBoard()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(1); // I-форма (1x4)
            tetromino.X = 3;
            tetromino.Y = -1; // Находится частично над полем, это нормальное явление для спавна

            // Не должно быть коллизий с границами поля, пока не пересекает левую/правую/нижнюю границу или занятую клетку
            Assert.False(board.IsCollision(tetromino));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IsCollision_ReturnsTrue_WhenPieceHitsRightBorder()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(1);
            tetromino.X = board.Width - tetromino.Shape.GetLength(1) + 1;
            tetromino.Y = 0;

            Assert.True(board.IsCollision(tetromino));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IsCollision_ReturnsTrue_WhenPieceHitsBottomBorder()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(0);
            tetromino.X = 3;
            tetromino.Y = board.Height - tetromino.Shape.GetLength(0) + 1;

            Assert.True(board.IsCollision(tetromino));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void IsCollision_ReturnsTrue_WhenOverlapsOccupiedCell()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(0);
            tetromino.X = 4;
            tetromino.Y = 10;

            Assert.False(board.IsCollision(tetromino));

            board.Grid[tetromino.Y, tetromino.X] = 1;

            Assert.True(board.IsCollision(tetromino));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MergeTetromino_WritesCellsInsideBoardOnly()
        {
            var board = TestHelpers.CreateDefaultBoard();
            var tetromino = TestHelpers.CreateTetromino(1);
            tetromino.X = 4;
            tetromino.Y = -1; // Часть фигуры над полем

            board.MergeTetromino(tetromino);

            int visibleBlocks = 0;
            for (int i = 0; i < tetromino.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < tetromino.Shape.GetLength(1); j++)
                {
                    if (tetromino.Shape[i, j] == 1 && tetromino.Y + i >= 0)
                    {
                        visibleBlocks++;
                    }
                }
            }

            Assert.Equal(visibleBlocks, TestHelpers.CountBlocks(board.Grid));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ClearLines_RemovesSingleFullRow_AndShiftsAboveDown()
        {
            var board = TestHelpers.CreateDefaultBoard();
            int bottom = board.Height - 1;
            for (int x = 0; x < board.Width; x++)
            {
                board.Grid[bottom, x] = 1;
            }

            board.Grid[bottom - 1, 0] = 1;

            int cleared = board.ClearLines();

            Assert.Equal(1, cleared);
            Assert.Equal(1, board.Grid[bottom, 0]);
            for (int x = 1; x < board.Width; x++)
            {
                Assert.Equal(0, board.Grid[bottom, x]);
            }

            for (int x = 0; x < board.Width; x++)
            {
                Assert.Equal(0, board.Grid[0, x]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ClearLines_RemovesMultipleFullRows()
        {
            var board = TestHelpers.CreateDefaultBoard();
            int bottom = board.Height - 1;

            for (int x = 0; x < board.Width; x++)
            {
                board.Grid[bottom, x] = 1;
                board.Grid[bottom - 1, x] = 1;
            }

            board.Grid[bottom - 2, 0] = 1;

            int cleared = board.ClearLines();

            Assert.Equal(2, cleared);
            Assert.Equal(1, board.Grid[bottom, 0]);
            for (int x = 1; x < board.Width; x++)
            {
                Assert.Equal(0, board.Grid[bottom, x]);
            }

            for (int x = 0; x < board.Width; x++)
            {
                Assert.Equal(0, board.Grid[bottom - 1, x]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ClearLines_RemovesTopmostLine_Correctly()
        {
            var board = TestHelpers.CreateDefaultBoard();

            // Заполняем самую верхнюю строку (y = 0)
            for (int x = 0; x < board.Width; x++)
            {
                board.Grid[0, x] = 1;
            }

            // Заполняем клетку под ней, чтобы проверить, что она не сдвинулась (ее некуда сдвигать вниз, так как очистилась строка над ней)
            board.Grid[1, 0] = 1;

            int cleared = board.ClearLines();

            Assert.Equal(1, cleared);
            // Верхняя строка очистилась
            for (int x = 0; x < board.Width; x++)
            {
                Assert.Equal(0, board.Grid[0, x]);
            }

            // Строка ниже осталась на месте
            Assert.Equal(1, board.Grid[1, 0]);
        }
    }
}
