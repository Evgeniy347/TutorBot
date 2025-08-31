using Shouldly; 
using TutorBot.TelegramService.BotActions;

namespace TutorBot.Test.Common
{
    public class WelcomeBotActionTest
    {
        [Fact]
        public void ExpandNumbers()
        {
            // Исходный список строк
            string[] inputList = [
            "РИ-151001",
            "РИ-421001/2/3",
            "РИ-421050/51/55",
            "РИ-511050/55",
            "РИ-601001/2"
            ];

            string[] outList =
            [
        "РИ-151001",

        "РИ-421001",
        "РИ-421002",
        "РИ-421003",

        "РИ-421050",
        "РИ-421051",
        "РИ-421055",

        "РИ-511050",
        "РИ-511055",

        "РИ-601001",
        "РИ-601002",
            ];

            // Формирование полного списка номеров
            var resultList = WelcomeBotAction.ExpandNumbers(inputList);

            string lineResult = resultList.OrderBy(x => x).JoinString(", ");
            string lineOut = outList.OrderBy(x => x).JoinString(", ");

            lineResult.ShouldBe(lineOut);
        }
    }
}
