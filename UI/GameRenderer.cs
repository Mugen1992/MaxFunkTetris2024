using System;

namespace MaxFunkTetris2024
{
    public class GameRenderer
    {
        // Статический метод для отрисовки текущего состояния игры
        public static void Render(GameBoard board, Tetromino currentTetromino, char blockChar)
        {
            // Рисуем построчно, чтобы избежать полной очистки консоли и мерцания
            for (int y = 0; y < board.Height; y++)
            {
                char[] row = new char[board.Width];

                for (int x = 0; x < board.Width; x++)
                {
                    bool isTetromino = false;
                    // Проверяем, находится ли текущий тетрамино в этой позиции
                    if (currentTetromino != null)
                    {
                        for (int i = 0; i < currentTetromino.Shape.GetLength(0); i++)
                        {
                            for (int j = 0; j < currentTetromino.Shape.GetLength(1); j++)
                            {
                                if (currentTetromino.Shape[i, j] == 1 &&
                                    x == currentTetromino.X + j &&
                                    y == currentTetromino.Y + i)
                                {
                                    isTetromino = true;
                                }
                            }
                        }
                    }

                    // Отрисовываем блок или пустое место
                    row[x] = (board.Grid[y, x] == 1 || isTetromino) ? blockChar : '.';
                }

                Console.SetCursorPosition(UiLayout.BoardOffsetX, UiLayout.BoardOffsetY + y);
                Console.Write(new string(row));
            }

            // Переводим курсор под поле, чтобы последующие WriteLine не попадали внутрь поля
            Console.SetCursorPosition(UiLayout.BoardOffsetX, UiLayout.BoardOffsetY + board.Height);
        }
    }
}
