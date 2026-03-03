using System;

namespace MaxFunkTetris2024
{
    public static class UiLayout
    {
        // Базовые размеры и смещения элементов интерфейса
        public const int LeftPanelX = 1;
        public const int LeftPanelY = 1;
        public const int LeftPanelWidth = 16;
        public const int LeftPanelHeight = 9;

        public const int BoardOffsetX = LeftPanelX + LeftPanelWidth + 3;
        public const int BoardOffsetY = 1;

        public const int PanelPadding = 3;
        public const int RightPanelWidth = 28;
        public const int RightPanelHeight = 9;

        public const int NextPanelWidth = 16;
        public const int NextPanelHeight = 6;

        public const int InventoryPanelWidth = 16;
        public const int InventoryPanelHeight = 5;

        // Координаты подписей статистики
        public static int StatsLabelX => LeftPanelX + 2;
        public static int StatsValueX => LeftPanelX + LeftPanelWidth - 7; // Сдвигаем левее, чтобы 6-значные числа не затирали рамку
        public static int ScoreLabelY => LeftPanelY + 2;
        public static int LevelLabelY => LeftPanelY + 3;
        public static int LinesLabelY => LeftPanelY + 4;

        // Координаты панелей, зависящие от ширины поля
        public static int GetInfoPanelX(int boardWidth) => BoardOffsetX + boardWidth + PanelPadding;
        public static int InfoPanelY => LeftPanelY;

        public static int GetNextPanelX(int boardWidth) => GetInfoPanelX(boardWidth);
        public static int GetNextPanelY() => InfoPanelY + RightPanelHeight + 1;

        public static int GetInventoryPanelX(int boardWidth) => GetNextPanelX(boardWidth);
        public static int GetInventoryPanelY() => GetNextPanelY() + NextPanelHeight + 1;
    }
}
