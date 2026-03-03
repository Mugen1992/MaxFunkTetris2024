using System;

namespace MaxFunkTetris2024
{
    public static class UiTheme
    {
        // Символы рамки
        public const char HorizontalBorder = '─';
        public const char VerticalBorder = '│';
        public const char CornerTopLeft = '┌';
        public const char CornerTopRight = '┐';
        public const char CornerBottomLeft = '└';
        public const char CornerBottomRight = '┘';

        // Цвета элементов интерфейса
        public const ConsoleColor FrameColor = ConsoleColor.DarkGray;
        public const ConsoleColor LabelColor = ConsoleColor.Gray;
        public const ConsoleColor ValueColor = ConsoleColor.Cyan;
        public const ConsoleColor InfoColor = ConsoleColor.White;
    }
}
