using System;

namespace MaxFunkTetris2024
{
    public static class UiStatsRenderer
    {
        // Обновляем только числовые значения статистики, подписи остаются статичными
        public static void RenderStats(int score, int level, int lines)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = UiTheme.ValueColor;

            Console.SetCursorPosition(UiLayout.StatsValueX, UiLayout.ScoreLabelY);
            Console.Write($"{score,6}");

            Console.SetCursorPosition(UiLayout.StatsValueX, UiLayout.LevelLabelY);
            Console.Write($"{level,6}");

            Console.SetCursorPosition(UiLayout.StatsValueX, UiLayout.LinesLabelY);
            Console.Write($"{lines,6}");

            Console.ForegroundColor = previousColor;
        }

        public static void RenderNextTetromino(Tetromino nextTetromino, int boardWidth, char blockChar)
        {
            int panelX = UiLayout.GetNextPanelX(boardWidth);
            int panelY = UiLayout.GetNextPanelY();

            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = UiTheme.ValueColor;

            // Очистка старой фигуры (просто закрашиваем пробелами внутреннюю область)
            for (int y = 0; y < 4; y++)
            {
                Console.SetCursorPosition(panelX + 2, panelY + 1 + y);
                Console.Write(new string(' ', UiLayout.NextPanelWidth - 4));
            }

            if (nextTetromino != null)
            {
                // Центрируем фигуру внутри панели
                int shapeWidth = nextTetromino.Shape.GetLength(1);
                int shapeHeight = nextTetromino.Shape.GetLength(0);
                int offsetX = (UiLayout.NextPanelWidth - shapeWidth) / 2;
                int offsetY = (UiLayout.NextPanelHeight - shapeHeight) / 2;

                for (int i = 0; i < shapeHeight; i++)
                {
                    for (int j = 0; j < shapeWidth; j++)
                    {
                        if (nextTetromino.Shape[i, j] == 1)
                        {
                            Console.SetCursorPosition(panelX + offsetX + j, panelY + offsetY + i);
                            Console.Write(blockChar);
                        }
                    }
                }
            }

            Console.ForegroundColor = previousColor;
        }

        public static void RenderInventory(PowerUpType currentPowerUp, int boardWidth)
        {
            int panelX = UiLayout.GetInventoryPanelX(boardWidth);
            int panelY = UiLayout.GetInventoryPanelY();

            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = UiTheme.ValueColor;

            // Очистка старого названия
            Console.SetCursorPosition(panelX + 2, panelY + 2);
            Console.Write(new string(' ', UiLayout.InventoryPanelWidth - 4));

            Console.SetCursorPosition(panelX + 2, panelY + 2);
            switch (currentPowerUp)
            {
                case PowerUpType.Bomb:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  [ BOMB ]  ");
                    break;
                case PowerUpType.Freeze:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(" [ FREEZE ] ");
                    break;
                case PowerUpType.None:
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  ( empty ) ");
                    break;
            }

            Console.ForegroundColor = previousColor;
        }
    }
}
