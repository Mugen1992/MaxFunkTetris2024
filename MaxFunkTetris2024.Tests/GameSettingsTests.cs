using System;
using System.IO;
using MaxFunkTetris2024;
using Xunit;
using System.Reflection;

namespace MaxFunkTetris2024.Tests
{
    public class GameSettingsTests : IDisposable
    {
        private readonly string _testSettingsFile = "settings.json";

        public GameSettingsTests()
        {
            if (File.Exists(_testSettingsFile))
            {
                File.Delete(_testSettingsFile);
            }
        }

        public void Dispose()
        {
            if (File.Exists(_testSettingsFile))
            {
                File.Delete(_testSettingsFile);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Load_ReturnsDefaultSettings_WhenFileDoesNotExist()
        {
            if (File.Exists(_testSettingsFile)) File.Delete(_testSettingsFile);

            var settings = GameSettings.Load();

            Assert.NotNull(settings);
            Assert.Equal(Difficulty.Normal, settings.Difficulty);
            Assert.Equal(GameMode.Classic, settings.Mode);
            Assert.Equal('█', settings.BlockChar);
            Assert.Equal(ConsoleKey.LeftArrow, settings.LeftKey);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Load_ReturnsDefaultSettings_WhenFileIsInvalid()
        {
            File.WriteAllText(_testSettingsFile, "invalid json");

            var settings = GameSettings.Load();

            Assert.NotNull(settings);
            Assert.Equal(Difficulty.Normal, settings.Difficulty);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SaveAndLoad_PersistsSettingsCorrectly()
        {
            var settings = new GameSettings
            {
                Difficulty = Difficulty.Hard,
                Mode = GameMode.Marathon,
                BlockChar = '#',
                LeftKey = ConsoleKey.A,
                RightKey = ConsoleKey.D,
                DownKey = ConsoleKey.S,
                RotateKey = ConsoleKey.W,
                PauseKey = ConsoleKey.Spacebar,
                PowerUpKey = ConsoleKey.Enter
            };

            settings.Save();

            Assert.True(File.Exists(_testSettingsFile));

            var loadedSettings = GameSettings.Load();

            Assert.Equal(Difficulty.Hard, loadedSettings.Difficulty);
            Assert.Equal(GameMode.Marathon, loadedSettings.Mode);
            Assert.Equal('#', loadedSettings.BlockChar);
            Assert.Equal(ConsoleKey.A, loadedSettings.LeftKey);
            Assert.Equal(ConsoleKey.D, loadedSettings.RightKey);
            Assert.Equal(ConsoleKey.S, loadedSettings.DownKey);
            Assert.Equal(ConsoleKey.W, loadedSettings.RotateKey);
            Assert.Equal(ConsoleKey.Spacebar, loadedSettings.PauseKey);
            Assert.Equal(ConsoleKey.Enter, loadedSettings.PowerUpKey);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Save_IgnoresExceptions_WhenFileCannotBeWritten()
        {
            // Чтобы вызвать ошибку записи (IOException/UnauthorizedAccessException),
            // мы можем заблокировать файл другим потоком.
            using (var stream = new FileStream(_testSettingsFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                var settings = new GameSettings();
                var exception = Record.Exception(() => settings.Save());

                // Метод Save игнорирует исключения в блоке catch
                Assert.Null(exception);
            }
        }
    }
}
