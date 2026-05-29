using Shouldly;

namespace System.Tests;

public class CheckTests
{
    [Fact]
    public void NotNull_WithValue_ReturnsValue()
    {
        object obj = new object();
        Check.NotNull(obj).ShouldBeSameAs(obj);
    }

    [Fact]
    public void NotNull_WithNull_Throws() =>
        Should.Throw<ArgumentNullException>(() => Check.NotNull<object>(null!));

    [Fact]
    public void NotNull_WithEmptyTitle_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotNull(new object(), ""));

    [Fact]
    public void NotEmpty_Array_WithItems_ReturnsArray()
    {
        int[] arr = new[] { 1, 2, 3 };
        Check.NotEmpty(arr).ShouldBe(arr);
    }

    [Fact]
    public void NotEmpty_Array_Empty_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotEmpty(Array.Empty<int>()));

    [Fact]
    public void NotEmpty_Array_Null_Throws() =>
        Should.Throw<ArgumentNullException>(() => Check.NotEmpty<int>(null!));

    [Fact]
    public void NotEmpty_String_WithText_ReturnsText()
    {
        string s = "hello";
        Check.NotEmpty(s).ShouldBe(s);
    }

    [Fact]
    public void NotEmpty_String_Null_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotEmpty(null!));

    [Fact]
    public void NotEmpty_String_Empty_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotEmpty(string.Empty));

    [Fact]
    public void NotEmpty_Guid_WithValue_ReturnsValue()
    {
        Guid g = Guid.NewGuid();
        Check.NotEmpty(g).ShouldBe(g);
    }

    [Fact]
    public void NotEmpty_Guid_Empty_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotEmpty(Guid.Empty));

    [Fact]
    public void NotEmpty_DateTime_WithValue_ReturnsValue()
    {
        DateTime dt = DateTime.UtcNow;
        Check.NotEmpty(dt).ShouldBe(dt);
    }

    [Fact]
    public void NotEmpty_DateTime_MinValue_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotEmpty(DateTime.MinValue));

    [Fact]
    public void NotZero_TimeSpan_WithValue_ReturnsValue()
    {
        TimeSpan ts = TimeSpan.FromSeconds(5);
        Check.NotZero(ts).ShouldBe(ts);
    }

    [Fact]
    public void NotZero_TimeSpan_Zero_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotZero(TimeSpan.Zero));

    [Fact]
    public void IsPositive_Int_WithPositive_ReturnsValue()
    {
        Check.IsPositive(5).ShouldBe(5);
    }

    [Fact]
    public void IsPositive_Int_Zero_Throws() =>
        Should.Throw<ArgumentException>(() => Check.IsPositive(0));

    [Fact]
    public void IsPositive_Int_Negative_Throws() =>
        Should.Throw<ArgumentException>(() => Check.IsPositive(-1));

    [Fact]
    public void NotZero_Int_WithValue_ReturnsValue()
    {
        Check.NotZero(5).ShouldBe(5);
    }

    [Fact]
    public void NotZero_Int_Zero_Throws() =>
        Should.Throw<ArgumentException>(() => Check.NotZero(0));

    [Fact]
    public void IsPositive_TimeSpan_WithPositive_ReturnsValue()
    {
        TimeSpan ts = TimeSpan.FromSeconds(1);
        Check.IsPositive(ts).ShouldBe(ts);
    }

    [Fact]
    public void IsPositive_TimeSpan_Zero_Throws() =>
        Should.Throw<ArgumentException>(() => Check.IsPositive(TimeSpan.Zero));

    [Fact]
    public void IsPositive_TimeSpan_Negative_Throws() =>
        Should.Throw<ArgumentException>(() => Check.IsPositive(TimeSpan.FromSeconds(-1)));

    [Fact]
    public void IsPositiveOrZero_Int_Positive_ReturnsValue()
    {
        Check.IsPositiveOrZero(5).ShouldBe(5);
    }

    [Fact]
    public void IsPositiveOrZero_Int_Zero_ReturnsValue()
    {
        Check.IsPositiveOrZero(0).ShouldBe(0);
    }

    [Fact]
    public void IsPositiveOrZero_Int_Negative_Throws() =>
        Should.Throw<ArgumentException>(() => Check.IsPositiveOrZero(-1));

    [Fact]
    public void IsPositiveOrZero_TimeSpan_Positive_ReturnsValue()
    {
        TimeSpan ts = TimeSpan.FromSeconds(1);
        Check.IsPositiveOrZero(ts).ShouldBe(ts);
    }

    [Fact]
    public void IsPositiveOrZero_TimeSpan_Zero_ReturnsValue()
    {
        Check.IsPositiveOrZero(TimeSpan.Zero).ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void IsPositiveOrZero_TimeSpan_Negative_Throws() =>
        Should.Throw<ArgumentException>(() => Check.IsPositiveOrZero(TimeSpan.FromSeconds(-1)));
}
