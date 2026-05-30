using Shouldly;
using TutorBot.TelegramService;

namespace TutorBot.Test.TelegramService;

[Trait("Category", "Unit")]
public class EvaluateKeyEvaluatorTests
{
    [Fact]
    public void Evaluate_QuotedString_ReturnsUnquoted()
    {
        var result = EvaluateKeyEvaluator.Evaluate("\"test123\"");

        result.ShouldBe("test123");
    }

    [Fact]
    public void Evaluate_QuotedEmptyString_ReturnsEmpty()
    {
        var result = EvaluateKeyEvaluator.Evaluate("\"\"");

        result.ShouldBe("");
    }

    [Fact]
    public void Evaluate_PlainString_ReturnsAsIs()
    {
        var result = EvaluateKeyEvaluator.Evaluate("eval-key");

        result.ShouldBe("eval-key");
    }

    [Fact]
    public void Evaluate_Null_ReturnsNull()
    {
        var result = EvaluateKeyEvaluator.Evaluate(null!);

        result.ShouldBeNull();
    }

    [Fact]
    public void Evaluate_EmptyString_ReturnsEmpty()
    {
        var result = EvaluateKeyEvaluator.Evaluate("");

        result.ShouldBe("");
    }

    [Fact]
    public void Evaluate_Whitespace_ReturnsAsIs()
    {
        var result = EvaluateKeyEvaluator.Evaluate("   ");

        result.ShouldBe("   ");
    }

    [Fact]
    public void Evaluate_ArithmeticAddition_ReturnsSum()
    {
        var result = EvaluateKeyEvaluator.Evaluate("3 + 4");

        result.ShouldBe(7);
    }

    [Fact]
    public void Evaluate_ArithmeticSubtraction_ReturnsDifference()
    {
        var result = EvaluateKeyEvaluator.Evaluate("10 - 3");

        result.ShouldBe(7);
    }

    [Fact]
    public void Evaluate_ArithmeticMultiplication_ReturnsProduct()
    {
        var result = EvaluateKeyEvaluator.Evaluate("6 * 7");

        result.ShouldBe(42);
    }

    [Fact]
    public void Evaluate_ArithmeticDivision_ReturnsQuotient()
    {
        var result = EvaluateKeyEvaluator.Evaluate("10 / 3");

        result.ShouldBe(3);
    }

    [Fact]
    public void Evaluate_ArithmeticOperatorPrecedence_MultiplicationBeforeAddition()
    {
        var result = EvaluateKeyEvaluator.Evaluate("2 + 3 * 4");

        result.ShouldBe(14);
    }

    [Fact]
    public void Evaluate_ArithmeticParentheses_OverridesPrecedence()
    {
        var result = EvaluateKeyEvaluator.Evaluate("(2 + 3) * 4");

        result.ShouldBe(20);
    }

    [Fact]
    public void Evaluate_ArithmeticComplex_NestedParentheses()
    {
        var result = EvaluateKeyEvaluator.Evaluate("((2 + 3) * (10 - 4)) / 5");

        result.ShouldBe(6);
    }

    [Fact]
    public void Evaluate_ArithmeticMultipleOperators_Chain()
    {
        var result = EvaluateKeyEvaluator.Evaluate("1 + 2 + 3 + 4");

        result.ShouldBe(10);
    }

    [Fact]
    public void Evaluate_DateTimeYear_ReturnsCurrentYear()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Year");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Year);
    }

    [Fact]
    public void Evaluate_DateTimeMonth_ReturnsCurrentMonth()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Month");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Month);
    }

    [Fact]
    public void Evaluate_DateTimeDay_ReturnsCurrentDay()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Day");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Day);
    }

    [Fact]
    public void Evaluate_DateTimeHour_ReturnsCurrentHour()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Hour");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Hour);
    }

    [Fact]
    public void Evaluate_DateTimeMinute_ReturnsCurrentMinute()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Minute");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Minute);
    }

    [Fact]
    public void Evaluate_DateTimeSecond_ReturnsCurrentSecond()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Second");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Second);
    }

    [Fact]
    public void Evaluate_DateTimeDayOfWeek_ReturnsCurrentDayOfWeek()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.DayOfWeek");

        result.ShouldBeOfType<int>();
        result.ShouldBe((int)DateTime.Now.DayOfWeek);
    }

    [Fact]
    public void Evaluate_DateTimeHourMultiplication_ReturnsCalculated()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Hour * 5");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Hour * 5);
    }

    [Fact]
    public void Evaluate_DateTimeMinutePlusHour_ReturnsCalculated()
    {
        var result = EvaluateKeyEvaluator.Evaluate("System.DateTime.Now.Minute + System.DateTime.Now.Hour");

        result.ShouldBeOfType<int>();
        result.ShouldBe(DateTime.Now.Minute + DateTime.Now.Hour);
    }

    [Fact]
    public void Evaluate_MissingClosingParenthesis_ThrowsInvalidOperation()
    {
        Should.Throw<InvalidOperationException>(() =>
            EvaluateKeyEvaluator.Evaluate("(2 + 3"));
    }

    [Fact]
    public void Evaluate_UnknownCharacter_ReturnsAsIs()
    {
        var result = EvaluateKeyEvaluator.Evaluate("2 + @");

        result.ShouldBe("2 + @");
    }
}
