using System.Text.Json;
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
    WaitingStartAuth,
    MainMenu,
    WaitingAlbumName,
    WaitingSortingMode,
    WaitingNewAlbumName,
}

public class BotConfig
{
    /// <summary>
    /// Telegram bot token. Received from <a href="https://t.me/BotFather">BotFather</a>
    /// </summary>
    public string BotToken { get; }

    /// <summary>
    /// Application ID. Received from <a href="https://vk.com/apps?act=manage ">VK app info</a>
    /// </summary>
    public long AppId { get; }

    /// <summary>
    /// Redirect URL. Should be specified in <a href="https://vk.com/apps?act=manage">VK app settings</a>
    /// </summary>
    public string RedirectUrl { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public BotConfig(string botToken, long appId, string redirectUrl)
    {
        BotToken = botToken;
        AppId = appId;
        RedirectUrl = redirectUrl;
    }
}

internal static class Program
{
    private static async Task Main()
    {
        const string configFilename = "bot_config.json";

        var logger = new LoggerConfiguration()
            .WriteTo.File("bot.log")
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();


        if (!File.Exists(configFilename))
        {
            logger.Fatal("Config file not found, exiting");
            return;
        }

        BotConfig botConfig;
        try
        {
            await using var configFile = File.OpenRead(configFilename);
            botConfig = JsonSerializer.Deserialize<BotConfig>(configFile, BotUtils.DefaultJsonOptions);
            ArgumentNullException.ThrowIfNull(botConfig);
        }
        catch (Exception e)
        {
            logger.Fatal(e.Message);
            logger.Fatal("Unable to parse config file, exiting");
            return;
        }

        while (true)
        {
            UserDatabase database;
            try
            {
                database = new UserDatabase("users.json");
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                logger.Fatal("Unable to read and parse user database, exiting");
                return;
            }

            TelegramController telegramController;
            try
            {
                telegramController = new TelegramController(botConfig, logger, database);
            }
            catch (ArgumentException)
            {
                logger.Fatal("Token '{token}' is invalid, exiting", botConfig.BotToken);
                return;
            }

            var vkManager = new VkManager(logger);

            try
            {
                telegramController.StartListening();
                telegramController.UserMessageReceived += async message =>
                    await message.Respond(logger, vkManager, telegramController);

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                logger.Error("Unhandled exception in message cycle!");
                logger.Error(e.Message);
                logger.Information("Full restart attempt");
            }
        }
    }
}