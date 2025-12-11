using Xunit;
using MaxFunkTetris2024;

namespace MaxFunkTetris2024.Tests
{
    // Юнит-тесты для вращений и сохранения состояния фигур
    public class TetrominoTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [Trait("Category", "Unit")]
        public void Rotate_FourTimes_ReturnsOriginalShape(int shapeIndex)
        {
            var tetromino = TestHelpers.CreateTetromino(shapeIndex);
            var original = (int[,])tetromino.Shape.Clone();

            for (int i = 0; i < 4; i++)
            {
                tetromino.Rotate();
            }

            Assert.True(TestHelpers.AreShapesEqual(original, tetromino.Shape));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [Trait("Category", "Unit")]
        public void Constructor_SetsInitialPosition_CenteredAtTop(int shapeIndex)
        {
            var tetromino = TestHelpers.CreateTetromino(shapeIndex);
            int width = tetromino.Shape.GetLength(1);

            Assert.Equal(0, tetromino.Y);
            Assert.Equal(5 - width / 2, tetromino.X);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [Trait("Category", "Unit")]
        public void SaveState_And_RestoreState_RestoreShapeAndPosition(int shapeIndex)
        {
            var tetromino = TestHelpers.CreateTetromino(shapeIndex);
            var saved = tetromino.SaveState();

            tetromino.X += 2;
            tetromino.Y += 3;
            tetromino.Rotate();

            tetromino.RestoreState(saved.shape, saved.x, saved.y);

            Assert.Equal(saved.x, tetromino.X);
            Assert.Equal(saved.y, tetromino.Y);
            Assert.True(TestHelpers.AreShapesEqual(saved.shape, tetromino.Shape));
        }
    }
}
