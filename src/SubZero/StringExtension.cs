using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SolidOps.SubZero
{
    public static class StringExtension
    {
        public static int Search(this String text, params string[] pValores)
        {
            int _ret = 0;
            try
            {
                var Palabras = text.Split(new char[] { ' ', '.', '?', ',', '!', '-', '(', ')', '"', '\'' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in Palabras)
                {
                    foreach (string palabra in pValores)
                    {
                        if (Regex.IsMatch(word, string.Format(@"\b{0}\b", palabra), RegexOptions.IgnoreCase))
                        {
                            _ret++;
                        }
                    }
                }
            }
            catch { }
            return _ret;
        }

        public static string ReplaceLine(this string content)
        {
            if (content.Contains("<line>"))
            {
                var currentDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var parts = currentDirectoryPath.Split('\\').ToList();
                var index = parts.IndexOf("Dev");
                if (index >= 0)
                {
                    var line = parts[index + 1].ToLower();
                    content = content.Replace("<line>", line);
                }
            }

            return Environment.ExpandEnvironmentVariables(content);
        }

        private static readonly string EscapeString = "/";

        public static string ToEscapedString(this string value, string delimiter)
        {
            var returnValue = value.Replace(EscapeString, EscapeString + EscapeString);
            returnValue = returnValue.Replace(delimiter, EscapeString + delimiter);
            return returnValue;
        }

        public static string ToUnescapedString(this string value, string delimiter)
        {
            var returnValue = value.Replace(EscapeString + delimiter, delimiter);
            returnValue = returnValue.Replace(EscapeString + EscapeString, EscapeString);
            return returnValue;
        }

        public static string ToQueryString(this object request, string separator = ",")
        {
            if (request == null)
                throw new ArgumentNullException("request");

            // Get all properties on the object
            var properties = request.GetType().GetProperties()
                .Where(x => x.CanRead)
                .Where(x => x.GetValue(request, null) != null)
                .ToDictionary(x => x.Name, x => x.GetValue(request, null));

            // Get names for all IEnumerable properties (excl. string)
            var propertyNames = properties
                .Where(x => !(x.Value is string) && x.Value is IEnumerable)
                .Select(x => x.Key)
                .ToList();

            // Concat all IEnumerable properties into a comma separated string
            foreach (var key in propertyNames)
            {
                var valueType = properties[key].GetType();
                var valueElemType = valueType.IsGenericType
                                        ? valueType.GetGenericArguments()[0]
                                        : valueType.GetElementType();
                if (valueElemType.IsPrimitive || valueElemType == typeof(string))
                {
                    var enumerable = properties[key] as IEnumerable;
                    properties[key] = string.Join(separator, enumerable.Cast<object>());
                }
            }

            // Concat all key/value pairs into a string separated by ampersand
            return string.Join("&", properties
                .Select(x => string.Concat(
                    Uri.EscapeDataString(x.Key), "=",
                    Uri.EscapeDataString(x.Value.ToString()))));
        }
    }
}
