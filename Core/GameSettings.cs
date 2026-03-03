using System;
using System.IO;
using System.Text.Json;

namespace MaxFunkTetris2024
{
    public class GameSettings
    {
        public Difficulty Difficulty { get; set; } = Difficulty.Normal;
        public GameMode Mode { get; set; } = GameMode.Classic;
        public char BlockChar { get; set; } = '█'; // █, ▓, ▒, ░, []
        public ConsoleKey LeftKey { get; set; } = ConsoleKey.LeftArrow;
        public ConsoleKey RightKey { get; set; } = ConsoleKey.RightArrow;
        public ConsoleKey DownKey { get; set; } = ConsoleKey.DownArrow;
        public ConsoleKey RotateKey { get; set; } = ConsoleKey.UpArrow;
        public ConsoleKey PauseKey { get; set; } = ConsoleKey.P;
        public ConsoleKey PowerUpKey { get; set; } = ConsoleKey.B;

        private const string SettingsFilePath = "settings.json";

        public static GameSettings Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<GameSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
                catch
                {
                    // Игнорируем ошибки чтения и возвращаем дефолтные настройки
                }
            }

            return new GameSettings();
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // Игнорируем ошибки записи
            }
        }
    }
}
