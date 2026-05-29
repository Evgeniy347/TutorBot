using Shouldly;

namespace System.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("ABC", "abc")]
    [InlineData("", "")]
    public void TryToLower_WithValue_ReturnsLower(string input, string expected) =>
        input.TryToLower().ShouldBe(expected);

    [Fact]
    public void TryToLower_Null_ReturnsEmpty() =>
        ((string)null!).TryToLower().ShouldBe(string.Empty);

    [Theory]
    [InlineData("Hello", "HELLO")]
    [InlineData("abc", "ABC")]
    [InlineData("", "")]
    public void TryToUpper_WithValue_ReturnsUpper(string input, string expected) =>
        input.TryToUpper().ShouldBe(expected);

    [Fact]
    public void TryToUpper_Null_ReturnsEmpty() =>
        ((string)null!).TryToUpper().ShouldBe(string.Empty);

    [Theory]
    [InlineData("  hello  ", "hello")]
    [InlineData("hello", "hello")]
    [InlineData("", "")]
    public void TryTrim_WithValue_ReturnsTrimmed(string input, string expected) =>
        input.TryTrim().ShouldBe(expected);

    [Fact]
    public void TryTrim_Null_ReturnsEmpty() =>
        ((string)null!).TryTrim().ShouldBe(string.Empty);

    [Fact]
    public void Replace_WithStringReplacement_ReplacesAll()
    {
        string result = "hello {name}".Replace("{name}", "world");
        result.ShouldBe("hello world");
    }

    [Fact]
    public void Replace_WithObjectReplacement_ReplacesAll()
    {
        string result = "value {x}".Replace("{x}", 42);
        result.ShouldBe("value 42");
    }

    [Fact]
    public void Replace_NullStr_ReturnsEmpty() =>
        System.StringExtensions.Replace(null!, "key", "value").ShouldBe(string.Empty);

    [Fact]
    public void ReplaceKey_ReplacesBracedKey()
    {
        string result = "Hello {UserName}".ReplaceKey("UserName", "Ivan");
        result.ShouldBe("Hello Ivan");
    }

    [Fact]
    public void ReplaceKey_MissingKey_ReturnsOriginal()
    {
        string result = "Hello {Name}".ReplaceKey("UserName", "Ivan");
        result.ShouldBe("Hello {Name}");
    }

    [Fact]
    public void ReplaceKey_NullValue_ReplacesWithEmpty()
    {
        string result = "Hello {Key}".ReplaceKey("Key", null!);
        result.ShouldBe("Hello ");
    }

    [Fact]
    public void ReplaceKey_NullStr_ReturnsNull() =>
        ((string)null!).ReplaceKey("key", "val").ShouldBeNull();

    [Fact]
    public void TryToLower_Null_ReturnsNull() =>
        ((string)null!).TryToLower().ShouldBe(string.Empty);

    [Fact]
    public void TryToLower_UpperCase_ReturnsLower() =>
        "HELLO".TryToLower().ShouldBe("hello");

    [Fact]
    public void TryToUpper_Null_ReturnsNull() =>
        ((string)null!).TryToUpper().ShouldBe(string.Empty);

    [Fact]
    public void TryToUpper_LowerCase_ReturnsUpper() =>
        "hello".TryToUpper().ShouldBe("HELLO");

    [Fact]
    public void TryTrim_Null_ReturnsNull() =>
        ((string)null!).TryTrim().ShouldBe(string.Empty);

    [Fact]
    public void TryTrim_WithSpaces_ReturnsTrimmed() =>
        "  hello  ".TryTrim().ShouldBe("hello");
}
