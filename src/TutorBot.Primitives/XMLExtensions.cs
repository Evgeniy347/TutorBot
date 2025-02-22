using System.Globalization; 
using System.Xml;

namespace WSS.Cryptography.Primitives.SYS.Lib.Primitives
{
    public static class XmlExtensions
    {
        public static bool GetBoolValue(this XmlNode xmlNode, string xPath, bool defaultValue = default, bool throwEmptyEx = false) =>
            GetValueParse(xmlNode, xPath, bool.Parse, defaultValue, throwEmptyEx);

        public static int GetIntValue(this XmlNode xmlNode, string xPath, int defaultValue = default, bool throwEmptyEx = false) =>
            GetValueParse(xmlNode, xPath, int.Parse, defaultValue, throwEmptyEx);

        public static double GetDoubleValue(this XmlNode xmlNode, string xPath, int defaultValue = default, bool throwEmptyEx = false) =>
            GetValueParse(xmlNode, xPath, DoubleParse, defaultValue, throwEmptyEx);

        public static Guid GetGuidValue(this XmlNode xmlNode, string xPath, Guid defaultValue = default, bool throwEmptyEx = false) =>
            GetValueParse(xmlNode, xPath, x => new Guid(x), defaultValue, throwEmptyEx);

        public static T? GetValueEnum<T>(this XmlNode xmlNode, string xPath, T? defaultValue = default, bool throwEmptyEx = false)
            where T : Enum =>
            GetValueParse(xmlNode, xPath, x => (T)Enum.Parse(typeof(T), x), defaultValue, throwEmptyEx);

        //public static int[] GetIntArrayValue(this XmlNode node, string xpath, int[]? defaultValue = default,
        //    string[]? separator = null, StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries, bool throwEmptyEx = false) =>
        //    GetValueParse(node, xpath, x => x?.Split(separator ?? new string[] { ";" }, splitOptions)?.Select(int.Parse)?.ToArray(), defaultValue, throwEmptyEx);

        //public static string[] GetStringArrayValue(this XmlNode node, string xpath, string[] defaultValue = default,
        //    string[] separator = null, StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries, bool throwEmptyEx = false) =>
        //    GetValueParse(node, xpath, x => x?.Split(separator ?? new string[] { ";" }, splitOptions), defaultValue, throwEmptyEx);

        public static T GetValueParse<T>(this XmlNode xmlNode, string xPath, Func<string, T> convert, T defaultValue, bool throwEmptyEx = false)
        {
            string stringValue = xmlNode.GetValue(xPath);

            try
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    if (throwEmptyEx)
                        throw new Exception($"empty value '{xPath}'");

                    return defaultValue;
                }

                return convert(stringValue);
            }
            catch (Exception ex)
            {
                throw new Exception($"parse error value '{stringValue}' xPath:{xPath}", ex);
            }
        }

        private static double DoubleParse(string doubleStr)
        {
            string numberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (numberDecimalSeparator != ".")
            {
                doubleStr = doubleStr.Replace(".", numberDecimalSeparator);
            }
            else if (numberDecimalSeparator != ",")
            {
                doubleStr = doubleStr.Replace(",", numberDecimalSeparator);
            }

            return double.Parse(doubleStr);
        }

        public static string GetValue(this XmlNode xmlNode, string xPath, string? defaultValue = null, bool throwEmptyEx = false)
        {
            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));

            string result = string.Empty;

            XmlNode? xmlNodeValue = xmlNode.SelectSingleNode(xPath);

            if (xmlNodeValue != null)
            {
                if (!string.IsNullOrEmpty(xmlNodeValue.Value))
                    result = xmlNodeValue.Value;

                else if (!string.IsNullOrEmpty(xmlNodeValue.InnerText))
                    result = xmlNodeValue.InnerText;
            }

            if (string.IsNullOrEmpty(result))
            {
                if (throwEmptyEx)
                    throw new Exception($"empty value '{xPath}'");

                return defaultValue ?? string.Empty;
            }

            return result;
        }
    }
}
