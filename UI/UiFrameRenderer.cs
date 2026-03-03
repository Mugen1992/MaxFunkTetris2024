using System;

namespace MaxFunkTetris2024
{
    public static class UiFrameRenderer
    {
        // Универсальный метод для рисования прямоугольной рамки
        public static void DrawBox(int x, int y, int width, int height, string title = "")
        {
            // Сохраняем текущий цвет, чтобы вернуть его после отрисовки
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = UiTheme.FrameColor;

            // Верхняя грань
            Console.SetCursorPosition(x, y);
            Console.Write(UiTheme.CornerTopLeft);
            Console.Write(new string(UiTheme.HorizontalBorder, width - 2));
            Console.Write(UiTheme.CornerTopRight);

            // Боковые грани
            for (int i = 1; i < height - 1; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(UiTheme.VerticalBorder);
                Console.SetCursorPosition(x + width - 1, y + i);
                Console.Write(UiTheme.VerticalBorder);
            }

            // Нижняя грань
            Console.SetCursorPosition(x, y + height - 1);
            Console.Write(UiTheme.CornerBottomLeft);
            Console.Write(new string(UiTheme.HorizontalBorder, width - 2));
            Console.Write(UiTheme.CornerBottomRight);

            // Заголовок рисуем один раз поверх рамки
            if (!string.IsNullOrWhiteSpace(title))
            {
                Console.SetCursorPosition(x + 2, y);
                Console.ForegroundColor = UiTheme.LabelColor;
                Console.Write(title);
            }

            Console.ForegroundColor = previousColor;
        }

        // Рисуем все статические рамки и подписи экранов во время игры
        public static void DrawStaticFrame(int boardWidth, int boardHeight)
        {
            Console.Clear();

            // Рисуем рамку вокруг игрового поля
            DrawBox(UiLayout.BoardOffsetX - 1, UiLayout.BoardOffsetY - 1, boardWidth + 2, boardHeight + 2, " FIELD ");

            // Левая панель статистики
            DrawBox(UiLayout.LeftPanelX, UiLayout.LeftPanelY, UiLayout.LeftPanelWidth, UiLayout.LeftPanelHeight, " STATS ");
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = UiTheme.LabelColor;
            Console.SetCursorPosition(UiLayout.StatsLabelX, UiLayout.ScoreLabelY);
            Console.Write("Score:");
            Console.SetCursorPosition(UiLayout.StatsLabelX, UiLayout.LevelLabelY);
            Console.Write("Level:");
            Console.SetCursorPosition(UiLayout.StatsLabelX, UiLayout.LinesLabelY);
            Console.Write("Lines:");

            // Правая панель информации
            int infoPanelX = UiLayout.GetInfoPanelX(boardWidth);
            DrawBox(infoPanelX, UiLayout.InfoPanelY, UiLayout.RightPanelWidth, UiLayout.RightPanelHeight, " INFO ");
            Console.ForegroundColor = UiTheme.InfoColor;
            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 2);
            Console.Write("←/→ — движение");
            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 3);
            Console.Write("↓ — ускорение падения");
            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 4);
            Console.Write("↑ — вращение фигуры");
            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 5);
            Console.Write("P — пауза");
            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 6);
            Console.Write("Esc — выход в меню");
            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 7);
            Console.Write("Enter — подтверждение");

            int nextPanelX = UiLayout.GetNextPanelX(boardWidth);
            int nextPanelY = UiLayout.GetNextPanelY();
            DrawBox(nextPanelX, nextPanelY, UiLayout.NextPanelWidth, UiLayout.NextPanelHeight, " NEXT ");

            int inventoryPanelX = UiLayout.GetInventoryPanelX(boardWidth);
            int inventoryPanelY = UiLayout.GetInventoryPanelY();
            DrawBox(inventoryPanelX, inventoryPanelY, UiLayout.InventoryPanelWidth, UiLayout.InventoryPanelHeight, " ITEM ");

            Console.SetCursorPosition(infoPanelX + 2, UiLayout.InfoPanelY + 8);
            Console.Write("B — исп. бонус");

            Console.ForegroundColor = previousColor;
        }

    }
}
