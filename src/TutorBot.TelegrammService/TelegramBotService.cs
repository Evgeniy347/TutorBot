using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options; 
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService
{
    internal class TelegramBotService(IApplication app, IOptions<TgBotServiceOptions> opt) : BackgroundService
    {
        private readonly TgBotServiceOptions _opt = opt.Value;
        private static bool _isRun;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_opt.Enable)
            {
                if (_isRun)
                    throw new Exception("is run");

                _isRun = true;


                TelegramBotClient botClient = new TelegramBotClient(_opt.Token, cancellationToken: stoppingToken);

                try
                {
                    User user = await botClient.GetMe();
                    await app.HistoryService.AddStatusService("Start", $"Id:{user.Id} FirstName:{user.FirstName}");

                    botClient.OnError += (exception, source) => ErrorHandle(exception, source, stoppingToken);
                    botClient.OnMessage += (message, type) => MessageHandle(message, type, botClient); 

                    await Task.Delay(-1, stoppingToken);
                }
                finally
                {
                    await botClient.Close(stoppingToken);
                    await app.HistoryService.AddStatusService("Stop");
                }
            }
        }


        private async Task MessageHandle(Message message, UpdateType type, TelegramBotClient botClient)
        {
            // Проверяем, что сообщение содержит текст
            if (message.Type == MessageType.Text)
            {
                ChatTransaction transaction = GetTransaction(message.Chat.Id);

                if (string.IsNullOrEmpty(transaction.GroupNumber))
                { 
                    if (string.IsNullOrEmpty(message.Text) || transaction.IsFirstMessage)
                    {
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: DialogTemplateMessage.GetHellowGroupNumber(),
                            replyMarkup: new ReplyKeyboardRemove()
                        );
                        transaction.IsFirstMessage = false;
                    }
                    else
                    {
                        if (_opt.GroupNumbers.Contains(message.Text, StringComparer.OrdinalIgnoreCase))
                        {
                            transaction.GroupNumber = message.Text;

                            // Отправляем сообщение с кнопками 
                            await botClient.SendMessage(message.Chat.Id, TextMessages.AskInterest, replyMarkup: GetMainMenuKeyboard());
                        }
                        else
                        {
                            await botClient.SendMessage(
                                chatId: message.Chat.Id,
                                text: DialogTemplateMessage.GetErrorGroupNumber(),
                                replyMarkup: new ReplyKeyboardRemove()
                            );
                        }
                    }
                }

                if (!string.IsNullOrEmpty(transaction.GroupNumber))
                {
                    var action = _handles.FirstOrDefault(a => a.Key == message.Text);
                    if (action != null)
                    {
                        await action.ExecuteAsync(transaction, botClient);
                    }
                    else
                    {
                        await botClient.SendMessage(message.Chat.Id, "Пожалуйста, выберите опцию из меню.");
                    }
                }
            }
        }

        public Task ErrorHandle(Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
            _ = app.HistoryService.AddHistory(new MessageHistory(-1, DateTime.Now, exception.ToString()));
            return Task.CompletedTask;
        }
          
        private IBotAction[] _handles = [
            new SimpleTextBotAction("Расписание группы", TextMessages.ScheduleLink),
            new SimpleTextBotAction("Найти преподавателя", TextMessages.FindTeacherLink),
            new SimpleTextBotAction("Количество академических задолженностей", TextMessages.AcademicDebtCountMessage),
            new SimpleTextBotAction("График пересдач", TextMessages.RetakeScheduleLink),
            new SimpleTextBotAction("Заказ хвостовки", TextMessages.OrderExamSheetLink),
            new SimpleTextBotAction("Я иностранный студент", TextMessages.ForeignStudentLink),
            new SimpleTextBotAction("Выбор секции ФК", TextMessages.FkSectionMessage),
            new SimpleTextBotAction("Прохождение тестирования на уровень владения иностранным языком", TextMessages.LanguageTestMessage),
            new SimpleSubMenuBotAction("Ликвидации академических задолженностей", GetDebtMenuKeyboard()),
            new SimpleSubMenuBotAction("Modeus", GetModeusMenuKeyboard()),
            new SimpleSubMenuBotAction("На главную", GetMainMenuKeyboard()),
            new ResetBotAction()
          ];

        private static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new[] { new KeyboardButton("Расписание группы") },
            new[] { new KeyboardButton("Найти преподавателя") },
            new[] { new KeyboardButton("Ликвидации академических задолженностей") },
            new[] { new KeyboardButton("Modeus") },
            new[] { new KeyboardButton("Я иностранный студент") },
            new[] { new KeyboardButton("Перезапустить") }
        });
        }

        private static ReplyKeyboardMarkup GetDebtMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new[] { new KeyboardButton("Количество академических задолженностей") },
            new[] { new KeyboardButton("График пересдач") },
            new[] { new KeyboardButton("Заказ хвостовки") },
            new[] { new KeyboardButton("На главную") }
        });
        }

        private static ReplyKeyboardMarkup GetModeusMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new[] { new KeyboardButton("Выбор секции ФК") },
            new[] { new KeyboardButton("Прохождение тестирования на уровень владения иностранным языком") },
            new[] { new KeyboardButton("На главную") }
        });
        }

        private ChatTransaction GetTransaction(long chatID)
        {
            return _tran ??= new ChatTransaction() { ChatID = chatID, IsFirstMessage = true };
        }

        ChatTransaction? _tran;
    }

    public static class TextMessages
    {
        public const string WelcomeMessage = "Добрый день/вечер. Пожалуйста, назовите номер группы в формате РИ-000000.";
        public const string AskInterest = "Что вас интересует?";
        public const string ScheduleLink = "Перейдите по ссылке: https://urfu.ru/ru/students/study/schedule/#/groups";
        public const string FindTeacherLink = "Перейдите по ссылке: https://urfu.ru/ru/students/study/schedule/#/groups";
        public const string AcademicDebtCountMessage = "Количество академических задолженностей (далее долгов) необходимо смотреть в зачетной книжке (зачетная книжка и БРС это разные вещи, вы должны проверить именно в зачетной книжке) à https://istudent.urfu.ru/services/ucheba";
        public const string RetakeScheduleLink = "Перейдите по ссылке: https://rtf.urfu.ru/resident-learning/retake/";
        public const string OrderExamSheetLink = "Перейдите по ссылке: http://docs.google.com/forms/d/1JMzq0Xou95CaQfm5rOuku3xKUsR7MUI_yz2sQr135w4/viewform?edit_requested=true";
        public const string ForeignStudentLink = "Перейдите по ссылке: https://urfu.ru/ru/international/centr-adaptacii-inostrannykh-obuchajushchikhsja/";
        public const string FkSectionMessage = "Ответственный Мусина Ольга Ивановна https://urfu.ru/ru/about/personal-pages/personal/person/olga.musina/";
        public const string LanguageTestMessage = "Ответственный Чернова Ольга Вячеславовна https://urfu.ru/ru/about/personal-pages/personal/person/o.v.chernova/";
    }

    public interface IBotAction
    {
        string Key { get; }
        Task ExecuteAsync(ChatTransaction transaction, ITelegramBotClient botClient);
    }

    public class SimpleTextBotAction : IBotAction
    {
        public string Key { get; } // Ключ действия
        private readonly string _text; // Текст для отправки

        public SimpleTextBotAction(string key, string text)
        {
            Key = key;
            _text = text;
        }

        public async Task ExecuteAsync(ChatTransaction transaction, ITelegramBotClient botClient)
        {
            await botClient.SendMessage(transaction.ChatID, _text);
        }
    }

    public class ResetBotAction : IBotAction
    {
        public string Key => "Перезапустить";
         
        public async Task ExecuteAsync(ChatTransaction transaction, ITelegramBotClient botClient)
        {
            transaction.GroupNumber = string.Empty; 
            await botClient.SendMessage(transaction.ChatID, DialogTemplateMessage.GetHellowGroupNumber(), replyMarkup: new ReplyKeyboardRemove()); 
        }
    }

    public class SimpleSubMenuBotAction : IBotAction
    {
        public string Key { get; } // Ключ действия
        private readonly ReplyKeyboardMarkup _subMenuKeyboard; // Клавиатура подменю

        public SimpleSubMenuBotAction(string key, ReplyKeyboardMarkup subMenuKeyboard)
        {
            Key = key;
            _subMenuKeyboard = subMenuKeyboard;
        }

        public async Task ExecuteAsync(ChatTransaction transaction, ITelegramBotClient botClient)
        {
            await botClient.SendMessage(transaction.ChatID, "Выберите опцию:", replyMarkup: _subMenuKeyboard);
        }
    }

    public class ChatTransaction
    {
        public long ChatID { get; set; }
        public string? GroupNumber { get; set; }
        public bool IsFirstMessage { get; set; }
    }

    public class DialogTemplateMessage
    {
        public static string GetHellowGroupNumber() => $"Добрый {GetTimeOfDay(DateTime.Now)}. Пожалуйста, назовите номер группы в формате РИ-000000";

        public static string GetErrorGroupNumber() => $"Указан некорректный номер группы. Пожалуйста, назовите номер группы в формате РИ-000000";

        private static string GetTimeOfDay(DateTime time)
        {
            int hour = time.Hour;

            if (hour >= 6 && hour < 12)
            {
                return "утро";
            }
            else if (hour >= 12 && hour < 18)
            {
                return "день";
            }
            else if (hour >= 18 && hour < 24)
            {
                return "вечер";
            }
            else
            {
                return "ночь";
            }
        }
    }
}
