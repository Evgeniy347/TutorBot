using System.Globalization;

namespace TutorBot.TelegramService;

internal static class EvaluateKeyEvaluator
{
    public static object Evaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression;

        expression = expression.Trim();

        if (expression.Length == 0)
            return expression;

        if (expression.Length >= 2 && expression[0] == '"' && expression[^1] == '"')
            return expression[1..^1];

        var processed = ReplaceDateTimeTokens(expression);

        if (processed.All(char.IsDigit))
            return int.Parse(processed, CultureInfo.InvariantCulture);

        if (processed.IndexOfAny(['+', '-', '*', '/', '(', ')']) >= 0
            && processed.All(c => char.IsDigit(c) || char.IsWhiteSpace(c) || IsOperatorOrParen(c)))
        {
            var pos = 0;
            return ParseExpression(processed.AsSpan(), ref pos);
        }

        return processed;
    }

    private static string ReplaceDateTimeTokens(string expr)
    {
        var now = DateTime.Now;
        return expr
            .Replace("System.DateTime.Now.DayOfWeek", ((int)now.DayOfWeek).ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("System.DateTime.Now.Year", now.Year.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("System.DateTime.Now.Month", now.Month.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("System.DateTime.Now.Day", now.Day.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("System.DateTime.Now.Hour", now.Hour.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("System.DateTime.Now.Minute", now.Minute.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("System.DateTime.Now.Second", now.Second.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private static bool IsOperatorOrParen(char c) => c is '+' or '-' or '*' or '/' or '(' or ')';

    private static int ParseExpression(ReadOnlySpan<char> expr, ref int pos)
    {
        var result = ParseTerm(expr, ref pos);
        while (pos < expr.Length)
        {
            SkipWhitespace(expr, ref pos);
            if (pos >= expr.Length) break;
            if (expr[pos] == '+') { pos++; result += ParseTerm(expr, ref pos); }
            else if (expr[pos] == '-') { pos++; result -= ParseTerm(expr, ref pos); }
            else break;
        }
        return result;
    }

    private static int ParseTerm(ReadOnlySpan<char> expr, ref int pos)
    {
        var result = ParseFactor(expr, ref pos);
        while (pos < expr.Length)
        {
            SkipWhitespace(expr, ref pos);
            if (pos >= expr.Length) break;
            if (expr[pos] == '*') { pos++; result *= ParseFactor(expr, ref pos); }
            else if (expr[pos] == '/') { pos++; result /= ParseFactor(expr, ref pos); }
            else break;
        }
        return result;
    }

    private static int ParseFactor(ReadOnlySpan<char> expr, ref int pos)
    {
        SkipWhitespace(expr, ref pos);
        if (pos >= expr.Length)
            throw new InvalidOperationException("Unexpected end of expression");

        if (expr[pos] == '(')
        {
            pos++;
            var result = ParseExpression(expr, ref pos);
            SkipWhitespace(expr, ref pos);
            if (pos >= expr.Length || expr[pos] != ')')
                throw new InvalidOperationException("Missing closing parenthesis");
            pos++;
            return result;
        }

        if (!char.IsDigit(expr[pos]))
            throw new InvalidOperationException($"Unexpected character '{expr[pos]}' at position {pos}");

        var start = pos;
        while (pos < expr.Length && char.IsDigit(expr[pos])) pos++;
        return int.Parse(expr[start..pos], CultureInfo.InvariantCulture);
    }

    private static void SkipWhitespace(ReadOnlySpan<char> expr, ref int pos)
    {
        while (pos < expr.Length && char.IsWhiteSpace(expr[pos])) pos++;
    }
}
