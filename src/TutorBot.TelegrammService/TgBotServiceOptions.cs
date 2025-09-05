namespace TutorBot.TelegramService
{
    internal class TgBotServiceOptions
    {
        public required bool Enable { get; init; }
        public required string Token { get; init; }
        public required string DialogModelPath { get; init; }
        //public required string AdminKey { get; init; }
        public required string EvaluateKey { get; init; }
    }
}
