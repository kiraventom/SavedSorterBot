using CLI.Messages;
using Serilog;
using Serilog.Events;

namespace CLI;

public enum SortingMode
{
    Random,
    ToOlder,
    ToNewer
}

public enum State
{
    WaitingStart,
    WaitingToken,
    MainMenu,
    WaitingAlbumName,
    WaitingSortingMode,
    WaitingNewAlbumName
}

internal static class Program
{
    private static async Task Main(string[] args)
    {
        const string botToken = "5432827470:AAHTbdf6RmZTxrGaA3zPCMWHA3h5Sm65pxs";

        while (true)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.File("saved_sorter_bot.log")
                .WriteTo.Console(LogEventLevel.Information)
                .CreateLogger();

            try
            {
                var database = new UserDatabase("users.json");

                var telegramController = new TelegramController(botToken, logger, database);
                var vkManager = new VkManager(logger);
                telegramController.UserMessageReceived += async message =>
                    await message.Respond(logger, vkManager, telegramController);

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                logger.Fatal("Root level exception occured!");
                logger.Fatal(e.Message);
                logger.Information("Full restart attempt");
            }
        }
    }
}