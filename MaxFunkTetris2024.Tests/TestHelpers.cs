using MaxFunkTetris2024;

namespace MaxFunkTetris2024.Tests
{
    public static class TestHelpers
    {
        // Фабрика для создания игрового поля с заданными размерами
        public static Program.GameBoard CreateBoard(int width, int height)
        {
            return new Program.GameBoard(width, height);
        }

        // Стандартная доска 10x20 для большинства тестов
        public static Program.GameBoard CreateDefaultBoard()
        {
            return CreateBoard(10, 20);
        }

        // Фабрика для получения тетрамино по индексу формы
        public static Program.Tetromino CreateTetromino(int shapeIndex)
        {
            return new Program.Tetromino(shapeIndex);
        }

        // Утилита для поклеточного сравнения двумерных массивов формы
        public static bool AreShapesEqual(int[,] left, int[,] right)
        {
            if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
            {
                return false;
            }

            for (int y = 0; y < left.GetLength(0); y++)
            {
                for (int x = 0; x < left.GetLength(1); x++)
                {
                    if (left[y, x] != right[y, x])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
