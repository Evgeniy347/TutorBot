namespace TutorBot.TelegramService
{
    internal class TgBotServiceOptions
    {
        public required bool Enable { get; init; }
        public required string Token { get; init; }
        public required string DialogModelPath { get; init; }

        public required string EvaluateKey { get; init; }

        public List<ProxySettings> Proxies { get; set; } = new();
    }

    public class ProxySettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string Type { get; set; } = "Http";  
    }
}
