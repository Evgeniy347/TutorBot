using Shouldly;

namespace System.Tests;

public class FormattingExtensionsTests
{
    [Fact]
    public void DoubleQuotes_String_Wraps() =>
        "hello".DoubleQuotes().ShouldBe("\"hello\"");

    [Fact]
    public void DoubleQuotes_Int_Wraps() =>
        42.DoubleQuotes().ShouldBe("\"42\"");

    [Fact]
    public void SingleQuotes_Wraps() =>
        "hello".SingleQuotes().ShouldBe("'hello'");

    [Fact]
    public void SquareBrackets_Wraps() =>
        "hello".SquareBrackets().ShouldBe("[hello]");

    [Fact]
    public void RoundBrackets_Wraps() =>
        "hello".RoundBrackets().ShouldBe("(hello)");

    [Fact]
    public void JoinStrings_WithSeparator_Joins()
    {
        var result = new[] { "a", "b", "c" }.JoinStrings(", ");
        result.ShouldBe("a, b, c");
    }

    [Fact]
    public void JoinStrings_DefaultSeparator_UsesComma()
    {
        var result = new[] { "a", "b" }.JoinStrings(",");
        result.ShouldBe("a,b");
    }

    [Fact]
    public void JoinStrings_NullValues_Throws() =>
        Should.Throw<ArgumentNullException>(() => ((IEnumerable<string>)null!).JoinStrings(","));

    [Fact]
    public void JoinString_Generic_Joins() =>
        new[] { 1, 2, 3 }.JoinString(", ").ShouldBe("1, 2, 3");

    [Fact]
    public void Trim_WithTrimValues_RemovesPrefixAndSuffix()
    {
        var result = "xxHelloWorldxx".Trim("xx");
        result.ShouldBe("HelloWorld");
    }

    [Fact]
    public void Trim_EmptyString_ReturnsEmpty() =>
        string.Empty.Trim("x").ShouldBe(string.Empty);

    [Fact]
    public void Trim_NullString_ReturnsNull() =>
        ((string)null!).Trim("x").ShouldBeNull();

    [Fact]
    public void Trim_NoTrimValues_ReturnsOriginal() =>
        "hello".Trim().ShouldBe("hello");

    [Fact]
    public void RemoveContentWhitespaces_WithIndentation_Removes()
    {
        var input = "\n  text\n  more";
        var result = input.RemoveContentWhitespaces();
        result.ShouldNotContain("\n  ");
    }

    [Fact]
    public void RemoveContentWhitespaces_Null_ReturnsNull() =>
        ((string)null!).RemoveContentWhitespaces().ShouldBeNull();

    [Fact]
    public void RemoveContentWhitespaces_Empty_ReturnsEmpty() =>
        string.Empty.RemoveContentWhitespaces().ShouldBe(string.Empty);

    [Fact]
    public void FullExceptionStack_ReturnsAllInnerExceptions()
    {
        var inner = new InvalidOperationException("inner");
        var outer = new Exception("outer", inner);

        var stack = outer.FullExceptionStack();
        stack.Count.ShouldBe(2);
        stack[0].Message.ShouldBe("outer");
        stack[1].Message.ShouldBe("inner");
    }

    [Fact]
    public void FullExceptionStack_Null_Throws() =>
        Should.Throw<ArgumentNullException>(() => ((Exception)null!).FullExceptionStack());

    [Fact]
    public void GetMessageStack_ReturnsFormattedMessages()
    {
        var inner = new InvalidOperationException("inner msg");
        var outer = new Exception("outer msg", inner);

        var result = outer.GetMessageStack();
        result.ShouldContain("outer msg");
        result.ShouldContain("inner msg");
    }

    [Fact]
    public void Trim_MultipleTrimValues_RemovesAll()
    {
        var result = "abchellodef".Trim("abc", "def");
        result.ShouldBe("hello");
    }

    [Fact]
    public void Trim_RepeatedTrim_RemovesNested()
    {
        var result = "xxyyxx".Trim("xx");
        result.ShouldBe("yy");
    }

    [Fact]
    public void JoinStrings_CharSeparator_Joins()
    {
        var result = new[] { "a", "b", "c" }.JoinStrings(',');
        result.ShouldBe("a,b,c");
    }

    [Fact]
    public void DoubleQuotes_IStringWrapper_ReturnsWrappedString()
    {
        var wrapper = new TestStringWrapper("test");
        wrapper.DoubleQuotes().ShouldBe("\"test\"");
    }

    [Fact]
    public void SingleQuotes_IStringWrapper_ReturnsWrappedString()
    {
        var wrapper = new TestStringWrapper("test");
        wrapper.SingleQuotes().ShouldBe("'test'");
    }

    [Fact]
    public void SquareBrackets_IStringWrapper_ReturnsWrappedString()
    {
        var wrapper = new TestStringWrapper("test");
        wrapper.SquareBrackets().ShouldBe("[test]");
    }

    [Fact]
    public void RoundBrackets_IStringWrapper_ReturnsWrappedString()
    {
        var wrapper = new TestStringWrapper("test");
        wrapper.RoundBrackets().ShouldBe("(test)");
    }

    [Fact]
    public void JoinStrings_IStringWrapperEnumerable_Joins()
    {
        var result = new IStringWrapper[] { new TestStringWrapper("a"), new TestStringWrapper("b") }.JoinStrings(", ");
        result.ShouldBe("a, b");
    }

    [Fact]
    public void ToLower_IStringWrapper_ReturnsLowercase()
    {
        var wrapper = new TestStringWrapper("HELLO");
        FormattingExtensions.ToLower(wrapper).ShouldBe("hello");
    }

    [Fact]
    public void Equals_IStringWrapper_String_ReturnsTrue() =>
        FormattingExtensions.Equals(new TestStringWrapper("hello"), "hello").ShouldBeTrue();

    [Fact]
    public void Equals_IStringWrapper_String_ReturnsFalse() =>
        FormattingExtensions.Equals(new TestStringWrapper("hello"), "world").ShouldBeFalse();

    [Fact]
    public void Equals_IStringWrapper_Null_ReturnsFalse() =>
        FormattingExtensions.Equals(new TestStringWrapper("hello"), null!).ShouldBeFalse();

    [Fact]
    public void Equals_IStringWrapper_String_Comparison_ReturnsTrue() =>
        new TestStringWrapper("Hello").Equals("hello", StringComparison.OrdinalIgnoreCase).ShouldBeTrue();

    [Fact]
    public void Equals_IStringWrapper_String_Comparison_ReturnsFalse() =>
        new TestStringWrapper("Hello").Equals("hello", StringComparison.Ordinal).ShouldBeFalse();

    private sealed class TestStringWrapper : IStringWrapper
    {
        private readonly string _value;
        public TestStringWrapper(string value) => _value = value;
        public override string ToString() => _value;
    }
}
