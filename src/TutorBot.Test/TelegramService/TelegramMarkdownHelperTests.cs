using Shouldly;
using TutorBot.TelegramService.BotActions;

namespace TutorBot.Test.TelegramService;

public class TelegramMarkdownHelperTests
{
    [Fact]
    public void EscapeMarkdownV2_PlainText_Unchanged() =>
        TelegramMarkdownHelper.EscapeMarkdownV2("Hello World").ShouldBe("Hello World");

    [Fact]
    public void EscapeMarkdownV2_Underscores_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("hello_world");
        result.ShouldBe("hello\\_world");
    }

    [Fact]
    public void EscapeMarkdownV2_Asterisks_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("hello*world");
        result.ShouldBe("hello\\*world");
    }

    [Fact]
    public void EscapeMarkdownV2_PreservesBoldMarkdown()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Text **bold** text");
        result.ShouldBe("Text **bold** text");
    }

    [Fact]
    public void EscapeMarkdownV2_PreservesItalicMarkdown()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Text _italic_ text");
        result.ShouldBe("Text _italic_ text");
    }

    [Fact]
    public void EscapeMarkdownV2_PreservesCodeMarkdown()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Text `code` text");
        result.ShouldBe("Text `code` text");
    }

    [Fact]
    public void EscapeMarkdownV2_PreservesPreMarkdown()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Text ```block``` text");
        result.ShouldBe("Text ```block``` text");
    }

    [Fact]
    public void EscapeMarkdownV2_PreservesStrikethrough()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Text ~~strike~~ text");
        result.ShouldBe("Text ~~strike~~ text");
    }

    [Fact]
    public void EscapeMarkdownV2_MixedContent_EscapesOutsideOnly()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Hello **bold** and _italic_ and `code`");
        result.ShouldBe("Hello **bold** and _italic_ and `code`");
    }

    [Fact]
    public void EscapeMarkdownV2_SpecialCharsOutsideMarkdown_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("Price: 10.99$ (discount!)");
        result.ShouldBe(@"Price: 10\.99$ \(discount\!\)");
    }

    [Fact]
    public void EscapeMarkdownV2_MixedMarkdownAndSpecialChars_EscapesOutside()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("**bold** _italic_ [link](url)");
        result.ShouldBe(@"**bold** _italic_ \[link\]\(url\)");
    }

    [Fact]
    public void EscapeMarkdownV2_BracketsOutsideMarkdown_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("x[y]z");
        result.ShouldBe("x\\[y\\]z");
    }

    [Fact]
    public void EscapeMarkdownV2_MultipleMarkdownFormats_Preserved()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("**_bold italic_** and ~~strike~~");
        result.ShouldBe("**_bold italic_** and ~~strike~~");
    }

    [Fact]
    public void EscapeMarkdownV2_EmptyString_ReturnsEmpty()
    {
        TelegramMarkdownHelper.EscapeMarkdownV2("").ShouldBe("");
    }

    [Fact]
    public void EscapeMarkdownV2_OnlySpecialChars_AllEscaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("_*[]()~`>#+-=|{}.!");
        result.ShouldBe(@"\_\*\[\]\(\)\~\`\>\#\+\-\=\|\{\}\.\!");
}

    [Fact]
    public void EscapeMarkdownV2_UnmatchedAsterisk_NotPreserved()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("not *bold");
        result.ShouldBe("not \\*bold");
    }

    [Fact]
    public void EscapeMarkdownV2_NumberSign_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("# header");
        result.ShouldBe("\\# header");
    }

    [Fact]
    public void EscapeMarkdownV2_Dash_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("- item");
        result.ShouldBe("\\- item");
    }

    [Fact]
    public void EscapeMarkdownV2_PlusSign_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("+ plus");
        result.ShouldBe("\\+ plus");
    }

    [Fact]
    public void EscapeMarkdownV2_EqualsSign_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("a=b");
        result.ShouldBe("a\\=b");
    }

    [Fact]
    public void EscapeMarkdownV2_Bang_Escaped()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2("hello!");
        result.ShouldBe("hello\\!");
    }

    [Fact]
    public void EscapeMarkdownV2_LeadingBackslash_DoesNotPreventEscaping()
    {
        var result = TelegramMarkdownHelper.EscapeMarkdownV2(@"\_");
        result.ShouldBe(@"\\_");
    }

    [Fact]
    public void EscapeMarkdownV2_NullText_Throws()
    {
        Should.Throw<ArgumentNullException>(() => TelegramMarkdownHelper.EscapeMarkdownV2(null!));
    }
}
