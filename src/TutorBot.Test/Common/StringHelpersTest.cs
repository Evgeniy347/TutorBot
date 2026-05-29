using Shouldly;
using TutorBot.TelegramService.Helpers;

namespace TutorBot.Test.Common
{
    [Trait("Category", "Unit")]
    public class StringHelpersTest
    {
        [Fact]
        public void ReplaceUserName_ThreeNames_ReturnsFirstGivenName()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", "иванов иван иванович").ShouldBe("Привет, иван!");
        }

        [Fact]
        public void ReplaceUserName_TwoNames_ReturnsSecondAsGivenName()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", "иванов иванович").ShouldBe("Привет, иванович!");
        }

        [Fact]
        public void ReplaceUserName_OneName_ReturnsItAsGivenName()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", "петр").ShouldBe("Привет, петр!");
        }

        [Fact]
        public void ReplaceUserName_TrailingWhitespace_Trims()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", "иванов  ").ShouldBe("Привет, иванов!");
        }

        [Fact]
        public void ReplaceUserName_BlankName_ReturnsUnknown()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", "  ").ShouldBe("Привет, неизвестный пользователь!");
        }

        [Fact]
        public void ReplaceUserName_NullName_ReturnsUnknown()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", null!).ShouldBe("Привет, неизвестный пользователь!");
        }

        [Fact]
        public void ReplaceUserName_EmptyName_ReturnsUnknown()
        {
            StringHelpers.ReplaceUserName("Привет, {UserName}!", "").ShouldBe("Привет, неизвестный пользователь!");
        }

        [Fact]
        public void ReplaceUserName_NoPlaceholder_ReturnsOriginal()
        {
            StringHelpers.ReplaceUserName("Просто текст", "иванов иван").ShouldBe("Просто текст");
        }

        [Fact]
        public void ExpandNumbers_NormalPatterns_Expands()
        {
            string[] inputList = [
                "РИ-151001",
                "РИ-421001/2/3",
                "РИ-421050/51/55",
                "РИ-511050/55",
                "РИ-601001/2"
            ];

            string[] outList = [
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

            string[] resultList = StringHelpers.ExpandNumbers(inputList);

            string lineResult = resultList.OrderBy(x => x).JoinString(", ");
            string lineOut = outList.OrderBy(x => x).JoinString(", ");

            lineResult.ShouldBe(lineOut);
        }

        [Fact]
        public void ExpandNumbers_SinglePattern_ReturnsItself()
        {
            string[] result = StringHelpers.ExpandNumbers(["РИ-151001"]);
            result.ShouldBe(["РИ-151001"]);
        }

        [Fact]
        public void ExpandNumbers_NoSlashes_ReturnsAll()
        {
            string[] result = StringHelpers.ExpandNumbers(["A", "B"]);
            result.ShouldBe(["A", "B"]);
        }

        [Fact]
        public void ExpandNumbers_EmptyInput_ReturnsEmpty()
        {
            string[] result = StringHelpers.ExpandNumbers([]);
            result.ShouldBeEmpty();
        }
    }
}
