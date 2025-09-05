using TutorBot.TelegramService.Helpers;
using Shouldly;

namespace TutorBot.Test.Common
{
    public class StringHelpersTest
    {
        [Fact]
        public void ReplaceUserName()
        {
            StringHelpers.ReplaceUserName("Отлично, {UserName}!", "иванов иван иванович").ShouldBe("Отлично, иван!");
            StringHelpers.ReplaceUserName("Отлично, {UserName}!", "иванов  иванович").ShouldBe("Отлично, иванович!");
            StringHelpers.ReplaceUserName("Отлично, {UserName}!", "иванов  ").ShouldBe("Отлично, иванов!");
            StringHelpers.ReplaceUserName("Отлично, {UserName}!", "  ").ShouldBe("Отлично, неизвестный пользователь!");
        }

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
            var resultList = StringHelpers.ExpandNumbers(inputList);

            string lineResult = resultList.OrderBy(x => x).JoinString(", ");
            string lineOut = outList.OrderBy(x => x).JoinString(", ");

            lineResult.ShouldBe(lineOut);
        }
    }
}
