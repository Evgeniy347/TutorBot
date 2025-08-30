using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.BotActions.Admins;

namespace TutorBot.TelegramService.BotActions
{
    internal static class TextMessages
    {
        public const string ErrorGroupNumber = $"Указан некорректный номер группы. Пожалуйста, назовите номер группы в формате РИ-000000";
        public const string WelcomeMessage = "Добрый день. Пожалуйста, назовите номер группы в формате РИ-000000.";
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

    internal class BotActionHub
    {
        public static IBotAction[] Handles { get; } = [
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
            ALBotAction.Instance,
            new AdminBotAction(),
            new ResetBotAction()
          ];

        public static IBotAction[] AdminHandles { get; } = [
            new StatisticBotAction(),
            new NotifyBotAction()
            ];

        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(
            [
                [ new KeyboardButton("Расписание группы") ],
                [ new KeyboardButton("Найти преподавателя") ],
                [ new KeyboardButton("Ликвидации академических задолженностей") ],
                [ new KeyboardButton("Modeus") ],
                [ new KeyboardButton("Я иностранный студент") ],
                //[ new KeyboardButton("Спросить нейросеть") ],
                [ new KeyboardButton("Перезапустить") ]
            ]);
        }

        public static ReplyKeyboardMarkup GetDebtMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(
            [
                [ new KeyboardButton("Количество академических задолженностей") ],
                [ new KeyboardButton("График пересдач") ],
                [ new KeyboardButton("Заказ хвостовки") ],
                [ new KeyboardButton("На главную") ]
            ]);
        }

        public static ReplyKeyboardMarkup GetModeusMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(
            [
                [ new KeyboardButton("Выбор секции ФК") ],
                [ new KeyboardButton("Прохождение тестирования на уровень владения иностранным языком") ],
                [ new KeyboardButton("На главную") ]
            ]);
        }

        public static ReplyKeyboardMarkup GetAdminMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(
            [
                [ new KeyboardButton("Получить статистику") ],
                [ new KeyboardButton("Оповещения об ошибках") ],
                [ new KeyboardButton("На главную") ]
            ]);
        }
    }
}
