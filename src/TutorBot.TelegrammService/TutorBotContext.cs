﻿using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService
{
    internal class TutorBotContext : IAsyncDisposable
    {
        private bool _disposed;

        public TutorBotContext(ITelegramBotClient client, IOptions<TgBotServiceOptions> opt, IApplication app)
        {
            Opt = opt.Value;
            App = app;
            Client = client;
        }

        public TgBotServiceOptions Opt { get; }

        public IApplication App { get; }

        public ITelegramBotClient Client { get; }

        private ChatEntry? _ChatEntry;
        public ChatEntry ChatEntry
        {
            get => Check.NotNull(_ChatEntry);
            set => _ChatEntry = Check.NotNull(value);
        }

        public async Task<Message> SendMessage(string text, ReplyMarkup? replyMarkup = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TutorBotContext));
             
            string logText = text;

            if (replyMarkup is null)
            {

            }
            else if (replyMarkup is ReplyKeyboardRemove)
            {
                logText += $@"
{new string('*', 10)} ReplyKeyboardRemove {new string('*', 10)}
";
            }
            else if (replyMarkup is ReplyKeyboardMarkup keyboardMarkup)
            {
                logText += $@"
{new string('*', 10)} ReplyKeyboardMarkup {new string('*', 10)}
{keyboardMarkup.Keyboard.SelectMany(x => x).Select(x => x.Text).JoinString("; ")}
";
            }

            _ = App.HistoryService.AddHistory(new MessageHistory(ChatEntry.ChatID, DateTime.Now, logText, "Bot", ChatEntry.NextCount()));

            return await Client.SendMessage(ChatEntry.ChatID, text, replyMarkup: replyMarkup);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_ChatEntry != null)
                {
                    await App.ChatService.Update(_ChatEntry);
                    _disposed = true;
                }
            }
        }
    }
}
