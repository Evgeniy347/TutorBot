using Telegram.Bot.Types.ReplyMarkups;

namespace TutorBot.TelegramService.BotActions
{
    internal class BotActionHub
    {
        public static IBotAction[] Handles = [
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
            new ResetBotAction()
          ];

        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new[] { new KeyboardButton("Расписание группы") },
            new[] { new KeyboardButton("Найти преподавателя") },
            new[] { new KeyboardButton("Ликвидации академических задолженностей") },
            new[] { new KeyboardButton("Modeus") },
            new[] { new KeyboardButton("Я иностранный студент") },
            new[] { new KeyboardButton("Спросить нейросеть") },
            new[] { new KeyboardButton("Перезапустить") }
        });
        }
         
        public static ReplyKeyboardMarkup GetDebtMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new[] { new KeyboardButton("Количество академических задолженностей") },
            new[] { new KeyboardButton("График пересдач") },
            new[] { new KeyboardButton("Заказ хвостовки") },
            new[] { new KeyboardButton("На главную") }
        });
        }

        public static ReplyKeyboardMarkup GetModeusMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new[] { new KeyboardButton("Выбор секции ФК") },
            new[] { new KeyboardButton("Прохождение тестирования на уровень владения иностранным языком") },
            new[] { new KeyboardButton("На главную") }
        });
        }
    }
}
