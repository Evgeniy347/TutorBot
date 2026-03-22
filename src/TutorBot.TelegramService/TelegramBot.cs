using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;

namespace TutorBot.TelegramService;

internal interface IBotFactory
{
    public Task<ITelegramBot> CreateBot(CancellationToken cancellationToken);
}

internal class BotFactory(TgBotServiceOptions options) : IBotFactory
{
    public async Task<ITelegramBot> CreateBot(CancellationToken cancellationToken)
    { 
        if (options.Proxies == null || !options.Proxies.Any())
        {
            var tgBotClient = new TelegramBotClient(options.Token, cancellationToken: cancellationToken);
            return new TelegramBot(tgBotClient);
        }
         
        foreach (var proxy in options.Proxies)
        {
            try
            {
                var httpClient = CreateHttpClientWithProxy(proxy);
                var tgBotClient = new TelegramBotClient(options.Token, httpClient, cancellationToken: cancellationToken);

                var botClient = new TelegramBot(tgBotClient);
                 
                await botClient.GetMe();

                Console.WriteLine($"[Proxy] Successfully connected via {proxy.Host}:{proxy.Port}");
                return botClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Proxy] Failed to connect via {proxy.Host}:{proxy.Port} - {ex.Message}"); 
            }
        }

        throw new Exception("Failed to connect via any proxy from the list");
    }

    private HttpClient CreateHttpClientWithProxy(ProxySettings proxy)
    {
        var httpClientHandler = new HttpClientHandler();

        if (!string.IsNullOrEmpty(proxy.Host))
        {
            var webProxy = new WebProxy(proxy.Host, proxy.Port)
            {
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
            {
                webProxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
            }

            httpClientHandler.Proxy = webProxy;
            httpClientHandler.UseProxy = true;
        }

        return new HttpClient(httpClientHandler);
    }
}


public class TelegramBot(TelegramBotClient botClient) : ITelegramBot
{
    public void AddErrorHandler(OnErrorHandler handler) => botClient.OnError += handler;

    public void AddMessageHandler(OnMessageHandler handler) => botClient.OnMessage += handler;

    public Task Close(CancellationToken stoppingToken) => botClient.Close(stoppingToken);

    public Task<User> GetMe() => botClient.GetMe();

    public Task<Message> SendMessage(
        ChatId chatId,
        string text,
        ParseMode parseMode = default,
        ReplyParameters? replyParameters = default,
        ReplyMarkup? replyMarkup = default,
        LinkPreviewOptions? linkPreviewOptions = default,
        int? messageThreadId = default,
        IEnumerable<MessageEntity>? entities = default,
        bool disableNotification = default,
        bool protectContent = default,
        string? messageEffectId = default,
        string? businessConnectionId = default,
        bool allowPaidBroadcast = default,
        CancellationToken cancellationToken = default
    ) => botClient.SendMessage(
        chatId: chatId,
        text: text,
        parseMode: parseMode,
        replyParameters: replyParameters,
        replyMarkup: replyMarkup,
        linkPreviewOptions: linkPreviewOptions,
        messageThreadId: messageThreadId,
        entities: entities,
        disableNotification: disableNotification,
        protectContent: protectContent,
        messageEffectId: messageEffectId,
        businessConnectionId: businessConnectionId,
        allowPaidBroadcast: allowPaidBroadcast,
        cancellationToken: cancellationToken);
}
