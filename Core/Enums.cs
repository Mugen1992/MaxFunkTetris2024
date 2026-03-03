using System;

namespace MaxFunkTetris2024
{
    // Состояния игры для управления основными экранами
    public enum GameState
    {
        MainMenu,
        Help,
        Settings,
        Leaderboard,
        Playing,
        Paused,
        GameOver
    }

    public enum PowerUpType
    {
        None,
        Bomb,       // Очищает 3 нижних ряда
        Freeze      // Замедляет падение на несколько ходов
    }

    // Уровни сложности для настройки скорости игры
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    // Режим игры
    public enum GameMode
    {
        Classic = 0,
        Marathon = 1
    }
}
