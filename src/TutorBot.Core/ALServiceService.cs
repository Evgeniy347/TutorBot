using LikhodedDynamics.Sber.GigaChatSDK;
using LikhodedDynamics.Sber.GigaChatSDK.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using TutorBot.Abstractions;

namespace TutorBot.Core
{
    internal class GigaChatService : IALService
    {
        private readonly GigaChatOptions _options;
        private readonly IServiceProvider _serviceProvider;

        private readonly GigaChat _gigaChat;
        internal GigaChatService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _options = serviceProvider.GetRequiredService<IOptions<GigaChatOptions>>().Value;
            _gigaChat = new GigaChat(_options.SecretKey, _options.IsCommercial, _options.IgnoreTLS, true);
        }

        public async Task<string> TransferQuestionAL(long chatID, string currentMessage, Guid sessionID)
        {
            await using (var scope = _serviceProvider.CreateAsyncScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DBChatEntry? chatDB = await dbContext.Chats.Where(x => x.ChatID == chatID).FirstOrDefaultAsync();

                if (chatDB == null)
                    throw new ArgumentException(nameof(chatID));

                MessageHistory[] histories = await dbContext.MessageHistories
                    .Where(x => x.SessionID == sessionID && x.ChatID == chatID && !string.IsNullOrWhiteSpace(x.Type))
                    .OrderBy(x => x.Id)
                    .ToArrayAsync();

                if (_gigaChat.Token == null)
                    if (await _gigaChat.CreateTokenAsync() == null)
                        throw new InvalidOperationException("CreateTokenAsync");

                MessageQuery messageQuery = BuildMessage(currentMessage, chatDB, histories);

                var result = await _gigaChat.CompletionsAsync(messageQuery);

                if (result == null)
                    throw new InvalidOperationException("CompletionsAsync");

                string? message = result?.choices?[0]?.message?.content;

                if (string.IsNullOrEmpty(message))
                    throw new InvalidOperationException("message");

                return message;
            }
        }

        private static MessageQuery BuildMessage(string currentMessage, DBChatEntry chatDB, MessageHistory[] histories)
        {
            MessageQuery messageQuery = new MessageQuery(null, "GigaChat:latest", 0.87f, 0.47f, 1L, stream: false, 512);

            messageQuery.messages.Add(new MessageContent("user", BuildPromt(chatDB)));

            foreach (MessageHistory history in histories)
            {
                string role = string.Empty;

                switch (history.Type)
                {
                    case "Client":
                        role = "user";
                        break;
                    case "Bot":
                        role = "assistant";
                        break;
                    default: throw new InvalidOperationException(role);
                }
                messageQuery.messages.Add(new MessageContent(role, history.MessageText));
            }

            messageQuery.messages.Add(new MessageContent("user", currentMessage));
            return messageQuery;
        }

        private static string BuildPromt(DBChatEntry chat)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@" 
Роль:Ты — виртуальный тьютор преподаватель, помогающий студентам разобраться в учебных вопросах. Твоя цель — понятно и доступно объяснить материал, поддержать мотивацию студента и сделать обучение интересным.

Задача:Ответь на следующий вопрос от студента по учебной дисциплине. Используй приведённую ниже основную информацию и предыдущие обсуждения, чтобы лучше понять ситуацию и предложить наиболее подходящий ответ.

Основная информация:  

Учебное заведение: ИРИТ-РТФ УрФУ
Учебный предмет: Информационная безопасность
Информация о студенте: FirstName - {chat.FirstName}, LastName - {chat.LastName}, UserName - {chat.UserName}

Требования к ответу:  

Четкость и структурированность изложения материала.
Использование понятных примеров и аналогий.
Мотивация студента продолжать учиться.
Ответ должен учитывать уровень подготовки и интересы студента.

База знаний:
   1. (кнопка, перейти по ссылке) Расписание группы https://urfu.ru/ru/students/study/schedule/#/groups
   1. (кнопка, перейти по ссылке) Найти преподавателя 
   1. (текст, перейти по ссылке) Количество академических задолженностей (далее долгов) необходимо смотреть в зачетной книжке (зачетная книжка и БРС это разные вещи, вы должны проверить именно в зачетной книжке) https://istudent.urfu.ru/services/ucheba
   1. (кнопка, перейти по ссылке) График пересдач https://rtf.urfu.ru/resident-learning/retake/
   1. (кнопка, перейти по ссылке) Заказ хвостовки (зачетно-экзаменационный лист) http://docs.google.com/forms/d/1JMzq0Xou95CaQfm5rOuku3xKUsR7MUI_yz2sQr135w4/viewform?edit_requested=true
   1. (кнопка, подменю) Modeus
   1. (текст с гиперссылкой) Выбор секции ФК (Ответственный Мусина Ольга Ивановна https://urfu.ru/ru/about/personal-pages/personal/person/olga.musina/)
   1. (текст с гиперссылкой) Прохождение тестирования на уровень владения иностранным языком (Ответственный Чернова Ольга Вячеславовна https://urfu.ru/ru/about/personal-pages/personal/person/o.v.chernova/)
   1. (кнопка, перейти по ссылке) Я иностранный студент https://urfu.ru/ru/international/centr-adaptacii-inostrannykh-obuchajushchikhsja/
");


            return builder.ToString();
        }
    }

    internal class GigaChatOptions
    {
        public required string SecretKey { get; init; }
        public required bool IsCommercial { get; init; }
        public required bool IgnoreTLS { get; init; }
    }
}
