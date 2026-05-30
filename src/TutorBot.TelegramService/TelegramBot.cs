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
        if (options.Proxies == null || options.Proxies.Count == 0)
        {
            TelegramBotClient tgBotClient = new TelegramBotClient(options.Token, cancellationToken: cancellationToken);
            return new TelegramBot(tgBotClient);
        }

        foreach (ProxySettings proxy in options.Proxies)
        {
            try
            {
#pragma warning disable CA2000 // HttpClient is passed to and owned by TelegramBotClient
                HttpClient httpClient = CreateHttpClientWithProxy(proxy);
                TelegramBotClient tgBotClient = new TelegramBotClient(options.Token, httpClient, cancellationToken: cancellationToken);
#pragma warning restore CA2000

                TelegramBot botClient = new TelegramBot(tgBotClient);

                await botClient.GetMe();

                Console.WriteLine($"[Proxy] Successfully connected via {proxy.Host}:{proxy.Port}");
                return botClient;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[Proxy] Failed to connect via {proxy.Host}:{proxy.Port} - {ex.Message}");
            }
        }

        throw new InvalidOperationException("Failed to connect via any proxy from the list");
    }

    private static HttpClient CreateHttpClientWithProxy(ProxySettings proxy)
    {
#pragma warning disable CA2000 // HttpClientHandler is disposed by HttpClient
        HttpClientHandler httpClientHandler = new HttpClientHandler
        {
            CheckCertificateRevocationList = true
        };

        if (proxy.BypassSslCheck)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        if (string.IsNullOrEmpty(proxy.Host))
            return new HttpClient(httpClientHandler);

        httpClientHandler.UseProxy = true;
        httpClientHandler.Proxy = proxy.Type.ToUpperInvariant() switch
        {
            "SOCKS5" => CreateSocks5Proxy(proxy),
            "HTTPS" => CreateHttpsProxy(proxy),
            _ => CreateHttpProxy(proxy)
        };

        return new HttpClient(httpClientHandler);
#pragma warning restore CA2000
    }

    private static WebProxy CreateHttpProxy(ProxySettings proxy)
    {
        WebProxy webProxy = new WebProxy(proxy.Host, proxy.Port)
        {
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false
        };
        SetProxyCredentials(webProxy, proxy);
        return webProxy;
    }

    private static WebProxy CreateHttpsProxy(ProxySettings proxy)
    {
        WebProxy webProxy = new WebProxy(new Uri($"https://{proxy.Host}:{proxy.Port}"))
        {
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false
        };
        SetProxyCredentials(webProxy, proxy);
        return webProxy;
    }

    private static WebProxy CreateSocks5Proxy(ProxySettings proxy)
    {
        UriBuilder uriBuilder = new UriBuilder("socks5", proxy.Host, proxy.Port);
        if (!string.IsNullOrEmpty(proxy.Username) || !string.IsNullOrEmpty(proxy.Password))
        {
            uriBuilder.UserName = proxy.Username ?? string.Empty;
            uriBuilder.Password = proxy.Password ?? string.Empty;
        }
        return new WebProxy(uriBuilder.Uri)
        {
            BypassProxyOnLocal = false
        };
    }

    private static void SetProxyCredentials(WebProxy webProxy, ProxySettings proxy)
    {
        if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
        {
            webProxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
        }
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
