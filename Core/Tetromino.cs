using System;

namespace MaxFunkTetris2024
{
    public class Tetromino
    {
        // Форма тетрамино, представленная двумерным массивом
        public int[,] Shape { get; private set; }

        // Координаты левого верхнего угла тетрамино на игровом поле
        public int X { get; set; }
        public int Y { get; set; }

        // Статический массив, содержащий все возможные формы тетрамино
        private static readonly int[][,] Shapes = new int[][,]
        {
            new int[,] { { 1, 1 }, { 1, 1 } },  // O-форма
            new int[,] { { 1, 1, 1, 1 } },      // I-форма
            new int[,] { { 0, 1, 1 }, { 1, 1, 0 } },  // Z-форма
            new int[,] { { 1, 1, 0 }, { 0, 1, 1 } },  // S-форма
            new int[,] { { 1, 0, 0 }, { 1, 1, 1 } },  // L-форма
            new int[,] { { 0, 0, 1 }, { 1, 1, 1 } },  // J-форма
            new int[,] { { 0, 1, 0 }, { 1, 1, 1 } }   // T-форма
        };

        // Конструктор, создающий новый тетрамино
        public Tetromino(int shapeIndex)
        {
            // Копируем форму из статического массива
            Shape = (int[,])Shapes[shapeIndex].Clone();

            // Устанавливаем начальную позицию тетрамино в верхней части поля
            X = 5 - Shape.GetLength(1) / 2;
            Y = 0;
        }

        // Сохраняем снимок текущей формы и координат, чтобы можно было вернуть состояние
        public (int[,] shape, int x, int y) SaveState()
        {
            return ((int[,])Shape.Clone(), X, Y);
        }

        // Возвращаем ранее сохранённую форму и позицию
        public void RestoreState(int[,] shape, int x, int y)
        {
            Shape = shape;
            X = x;
            Y = y;
        }

        // Метод для вращения тетрамино
        public void Rotate()
        {
            // Создаем новый массив с инвертированными размерами
            int[,] rotated = new int[Shape.GetLength(1), Shape.GetLength(0)];

            // Заполняем новый массив, поворачивая исходную форму на 90 градусов
            for (int i = 0; i < Shape.GetLength(0); i++)
                for (int j = 0; j < Shape.GetLength(1); j++)
                    rotated[j, Shape.GetLength(0) - 1 - i] = Shape[i, j];

            // Заменяем исходную форму на повернутую
            Shape = rotated;
        }
    }
}
