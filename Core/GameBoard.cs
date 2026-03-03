using System;

namespace MaxFunkTetris2024
{
    public class GameBoard
    {
        // Сетка игрового поля
        public int[,] Grid { get; private set; }

        // Размеры игрового поля
        public int Width { get; }
        public int Height { get; }

        // Конструктор, создающий новое игровое поле
        public GameBoard(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new int[height, width];
        }

        // Метод для проверки столкновения тетрамино с границами поля или другими блоками
        public bool IsCollision(Tetromino tetromino)
        {
            for (int i = 0; i < tetromino.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < tetromino.Shape.GetLength(1); j++)
                {
                    if (tetromino.Shape[i, j] == 1)
                    {
                        int x = tetromino.X + j;
                        int y = tetromino.Y + i;
                        // Проверяем выход за границы поля или наличие блока в сетке
                        if (x < 0 || x >= Width || y >= Height || (y >= 0 && Grid[y, x] == 1))
                            return true;
                    }
                }
            }
            return false;
        }

        // Метод для слияния тетрамино с игровым полем
        public void MergeTetromino(Tetromino tetromino)
        {
            for (int i = 0; i < tetromino.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < tetromino.Shape.GetLength(1); j++)
                {
                    if (tetromino.Shape[i, j] == 1)
                    {
                        int x = tetromino.X + j;
                        int y = tetromino.Y + i;
                        if (y >= 0 && y < Height && x >= 0 && x < Width)
                            Grid[y, x] = 1;
                    }
                }
            }
        }

        // Возвращает индексы линий, которые полностью заполнены
        public System.Collections.Generic.List<int> GetLinesToClear()
        {
            var lines = new System.Collections.Generic.List<int>();
            for (int y = Height - 1; y >= 0; y--)
            {
                if (IsLineFull(y))
                {
                    lines.Add(y);
                }
            }
            return lines;
        }

        // Очищает и сдвигает переданные линии
        public void ClearSpecificLines(System.Collections.Generic.List<int> lines)
        {
            // Сортируем линии от нижних к верхним
            lines.Sort((a, b) => b.CompareTo(a));

            for (int k = 0; k < lines.Count; k++)
            {
                int y = lines[k];
                ClearLine(y);
                ShiftLinesDown(y);

                // Так как мы сдвинули линии вниз, все индексы выше удаленной линии увеличиваются на 1
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i] < y)
                    {
                        lines[i]++;
                    }
                }
            }
        }

        // Метод для очистки заполненных линий и возврата количества очищенных строк
        public int ClearLines()
        {
            var linesToClear = GetLinesToClear();
            if (linesToClear.Count > 0)
            {
                ClearSpecificLines(linesToClear);
            }
            return linesToClear.Count;
        }

        // Метод для проверки, заполнена ли линия
        private bool IsLineFull(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                if (Grid[y, x] == 0)
                    return false;
            }
            return true;
        }

        // Метод для очистки линии
        private void ClearLine(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                Grid[y, x] = 0;
            }
        }

        // Метод для сдвига всех линий выше очищенной вниз
        private void ShiftLinesDown(int clearedLine)
        {
            for (int y = clearedLine - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    Grid[y + 1, x] = Grid[y, x];
                }
            }

            // После сдвига очищаем верхнюю строку, чтобы не тянуть «мусор» сверху
            for (int x = 0; x < Width; x++)
            {
                Grid[0, x] = 0;
            }
        }
    }
}
