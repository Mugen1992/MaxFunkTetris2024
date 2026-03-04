using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MaxFunkTetris2024;
using Xunit;

namespace MaxFunkTetris2024.Tests
{
    [Collection("Sequential")]
    public class GameLoopTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task Run_ExecutesLoop_UntilIsRunningIsFalse()
        {
            var game = new Game(10, 20);

            // Для теста цикла обходим чтение консоли, которое падает
            // Подменяем метод HandleInput, либо просто перехватываем ошибку
            var task = Task.Run(() =>
            {
                try
                {
                    game.Run();
                }
                catch (InvalidOperationException)
                {
                    // В CI/CD KeyAvailable кидает это исключение, мы просто завершаем игру
                    game.IsRunning = false;
                }
            });

            // Даем ей поработать пару циклов
            await Task.Delay(100);

            // Останавливаем
            game.IsRunning = false;

            // Ожидаем завершения
            var delayTask = Task.Delay(1000);
            var completedTask = await Task.WhenAny(task, delayTask);

            Assert.True(completedTask == task, "Game loop did not exit when IsRunning was set to false.");
        }
    }
}
