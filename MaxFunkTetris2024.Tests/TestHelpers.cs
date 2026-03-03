using MaxFunkTetris2024;

namespace MaxFunkTetris2024.Tests
{
    internal static class TestHelpers
    {
        // Фабрика для создания игрового поля с заданными размерами
        internal static GameBoard CreateBoard(int width, int height)
        {
            return new GameBoard(width, height);
        }

        // Стандартная доска 10x20 для большинства тестов
        internal static GameBoard CreateDefaultBoard()
        {
            return CreateBoard(10, 20);
        }

        // Фабрика для получения тетрамино по индексу формы
        internal static Tetromino CreateTetromino(int shapeIndex)
        {
            return new Tetromino(shapeIndex);
        }

        // Утилита для поклеточного сравнения двумерных массивов формы
        internal static bool AreShapesEqual(int[,] left, int[,] right)
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

        // Подсчёт количества занятых клеток в матрице
        internal static int CountBlocks(int[,] matrix)
        {
            int blocks = 0;
            for (int y = 0; y < matrix.GetLength(0); y++)
            {
                for (int x = 0; x < matrix.GetLength(1); x++)
                {
                    blocks += matrix[y, x];
                }
            }

            return blocks;
        }
    }
}
