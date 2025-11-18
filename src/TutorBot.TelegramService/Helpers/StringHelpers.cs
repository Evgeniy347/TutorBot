
namespace TutorBot.TelegramService.Helpers
{
    internal class StringHelpers
    {
        internal static string ReplaceUserName(string sourceText, string fullName)
        {
            string text;
            if (!string.IsNullOrEmpty(fullName?.Trim()))
            {
                string[] fioParts = fullName.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (fioParts.Length > 0)
                {
                    string name = fioParts.Skip(1).FirstOrDefault() ?? fioParts.First();

                    text = sourceText.Replace("{UserName}", name);
                }
                else
                {
                    text = sourceText.Replace("{UserName}", "неизвестный пользователь");
                }
            }
            else
            {
                text = sourceText.Replace("{UserName}", "неизвестный пользователь");
            }

            return text;
        }

        internal static string[] ExpandNumbers(string[] inputList)
        {
            List<string> outputList = new List<string>();

            foreach (string str in inputList)
            {
                string[] parts = str.Split('/');

                outputList.Add(parts[0]);

                if (parts.Length > 1)
                {
                    string source = parts[0];
                    foreach (string numPart in parts.Skip(1))
                    {
                        string group = source.Remove(source.Length - numPart.Length) + numPart;
                        outputList.Add(group);
                    }
                }
            }

            return [.. outputList];
        }
    }
}
