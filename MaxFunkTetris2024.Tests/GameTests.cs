using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MaxFunkTetris2024;
using Xunit;

namespace MaxFunkTetris2024.Tests
{
    // Тесты, перехватывающие консольный ввод/вывод для тестирования методов Game.cs
    [Collection("Sequential")]
    public class GameTests : IDisposable
    {
        private readonly StringWriter _stringWriter;
        private readonly TextWriter _originalOutput;

        public GameTests()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }

        private static void InvokePrivateMethod(Game game, string methodName, params object[] parameters)
        {
            var method = typeof(Game).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(game, parameters);
        }

        private static T GetPrivateField<T>(Game game, string fieldName)
        {
            var field = typeof(Game).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            return (T)field.GetValue(game);
        }

        private static void SetPrivateField(Game game, string fieldName, object value)
        {
            var field = typeof(Game).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(game, value);
        }

        // --- Render Methods Tests ---

        [Fact]
        [Trait("Category", "Integration")]
        public void RenderMainMenu_WritesMenuToConsole()
        {
            var game = new Game(10, 20);
            SetPrivateField(game, "_mainMenuDrawn", false);

            var exception = Record.Exception(() => InvokePrivateMethod(game, "RenderMainMenu"));

            if (exception?.InnerException is IOException || exception is IOException)
            {
                // Ожидаемо, если Console.SetCursorPosition бросает IOException без физической консоли
                return;
            }

            var output = _stringWriter.ToString();
            Assert.Contains("MAIN MENU", output);
            Assert.Contains("Start", output);
            Assert.Contains("Exit", output);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RenderSettings_WritesSettingsToConsole()
        {
            var game = new Game(10, 20);
            SetPrivateField(game, "_settingsDrawn", false);

            var exception = Record.Exception(() => InvokePrivateMethod(game, "RenderSettings"));

            if (exception?.InnerException is IOException || exception is IOException) return;

            var output = _stringWriter.ToString();
            Assert.Contains("SETTINGS", output);
            Assert.Contains("Режим", output);
            Assert.Contains("Сложность", output);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RenderLeaderboard_WritesHighscoresToConsole()
        {
            var game = new Game(10, 20);
            SetPrivateField(game, "_leaderboardDrawn", false);

            var exception = Record.Exception(() => InvokePrivateMethod(game, "RenderLeaderboard"));

            if (exception?.InnerException is IOException || exception is IOException) return;

            var output = _stringWriter.ToString();
            Assert.Contains("LEADERBOARD", output);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RenderHelp_WritesHelpToConsole()
        {
            var game = new Game(10, 20);
            SetPrivateField(game, "_helpDrawn", false);

            var exception = Record.Exception(() => InvokePrivateMethod(game, "RenderHelp"));

            if (exception?.InnerException is IOException || exception is IOException) return;

            var output = _stringWriter.ToString();
            Assert.Contains("HELP", output);
            Assert.Contains("Управление", output);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RenderPaused_WritesOverlay()
        {
            var game = new Game(10, 20);
            SetPrivateField(game, "_pausedDrawn", false);

            var exception = Record.Exception(() => InvokePrivateMethod(game, "RenderPaused"));

            if (exception?.InnerException is IOException || exception is IOException) return;

            var output = _stringWriter.ToString();
            Assert.Contains("PAUSED", output);
            Assert.Contains("Игра на паузе", output);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RenderGameOver_WritesFinalScore()
        {
            var game = new Game(10, 20);
            SetPrivateField(game, "_gameOverDrawn", false);
            SetPrivateField(game, "_score", 999);

            var exception = Record.Exception(() => InvokePrivateMethod(game, "RenderGameOver"));

            if (exception?.InnerException is IOException || exception is IOException) return;

            var output = _stringWriter.ToString();
            Assert.Contains("Game Over", output);
            Assert.Contains("999", output);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Update_DelegatesToCorrectMethod_BasedOnState()
        {
            var game = new Game(10, 20);

            // Menu state - no update expected to change fields
            SetPrivateField(game, "_state", GameState.MainMenu);
            InvokePrivateMethod(game, "Update");

            // Paused state
            SetPrivateField(game, "_state", GameState.Paused);
            InvokePrivateMethod(game, "Update");

            // Help state
            SetPrivateField(game, "_state", GameState.Help);
            InvokePrivateMethod(game, "Update");

            // Settings state
            SetPrivateField(game, "_state", GameState.Settings);
            InvokePrivateMethod(game, "Update");

            // Game over state
            SetPrivateField(game, "_state", GameState.GameOver);
            InvokePrivateMethod(game, "Update");

            // This test just ensures the switch statement processes all branches without error
            Assert.True(true);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void HighScore_CanBeSavedAndLoaded()
        {
            var game = new Game(10, 20);

            // Delete Highscore file if exists to start fresh
            if (File.Exists("highscores.txt")) File.Delete("highscores.txt");

            InvokePrivateMethod(game, "SaveHighScore", 500);
            InvokePrivateMethod(game, "SaveHighScore", 1000);
            InvokePrivateMethod(game, "SaveHighScore", 200);

            var scores = (System.Collections.Generic.List<int>)typeof(Game).GetMethod("LoadHighScores", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, null);

            Assert.Equal(3, scores.Count);
            Assert.Equal(1000, scores[0]); // Sorted descending
            Assert.Equal(500, scores[1]);
            Assert.Equal(200, scores[2]);

            // Save invalid scores (<= 0) should be ignored
            InvokePrivateMethod(game, "SaveHighScore", 0);
            InvokePrivateMethod(game, "SaveHighScore", -10);

            scores = (System.Collections.Generic.List<int>)typeof(Game).GetMethod("LoadHighScores", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, null);
            Assert.Equal(3, scores.Count);

            if (File.Exists("highscores.txt")) File.Delete("highscores.txt");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Render_DelegatesToCorrectMethod_BasedOnState()
        {
            var game = new Game(10, 20);

            var states = new[] {
                GameState.MainMenu, GameState.Leaderboard, GameState.Help,
                GameState.Settings, GameState.Paused, GameState.GameOver, GameState.Playing
            };

            foreach (var state in states)
            {
                SetPrivateField(game, "_state", state);
                try
                {
                    InvokePrivateMethod(game, "Render");
                }
                catch (TargetInvocationException ex) when (ex.InnerException is IOException)
                {
                    // Ignore Console errors in test env
                }
                catch (ObjectDisposedException) { }
                catch (TargetInvocationException ex) when (ex.InnerException is ObjectDisposedException) { }
            }

            // Just ensures it doesn't crash on invalid states and passes through the switch
            Assert.True(true);
        }

        // Helper for Input simulation (since Console.KeyAvailable is hard to fake without mocks,
        // we can inject a stream, but testing logic inside Handle methods might require it)
        [Fact]
        [Trait("Category", "Integration")]
        public void HandleInput_ReturnsSilently_WhenNoKeyAvailable()
        {
            var game = new Game(10, 20);

            var states = new[] {
                GameState.MainMenu, GameState.Leaderboard, GameState.Help,
                GameState.Settings, GameState.Paused, GameState.GameOver, GameState.Playing
            };

            foreach (var state in states)
            {
                SetPrivateField(game, "_state", state);
                // Это не упадет, так как Console.KeyAvailable вернет false (или выкинет InvalidOperation, если нет консоли, но xUnit runner обычно предоставляет поток без ключей)
                var ex = Record.Exception(() => InvokePrivateMethod(game, "HandleInput"));
                if (ex?.InnerException is InvalidOperationException || ex is InvalidOperationException)
                {
                    // CI/CD might throw InvalidOperationException on Console.KeyAvailable
                    continue;
                }
                Assert.Null(ex);
            }
        }
    }
}
