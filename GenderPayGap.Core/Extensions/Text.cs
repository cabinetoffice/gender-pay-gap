using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GenderPayGap.Extensions
{
    public static class Text
    {

        public static bool IsNumber(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return Regex.IsMatch(input, "^\\d+$");
        }

        public static bool IsNullOrWhiteSpace(this string input, params string[] inputs)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            if (inputs.Any(i => string.IsNullOrWhiteSpace(i)))
            {
                return true;
            }

            return false;
        }

        public static string TrimI(this string source, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return trimChars == null || trimChars.Length == 0 ? source.Trim() : source.Trim(trimChars);
        }

        public static bool EqualsI(this string original, params string[] target)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                original = "";
            }

            for (var i = 0; i < target.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(target[i]))
                {
                    target[i] = "";
                }

                if (original.Equals(target[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsI(this string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            return source.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool StartsWithI(this string original, params string[] texts)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                return false;
            }

            if (texts != null)
            {
                foreach (string text in texts)
                {
                    if (text != null && original.ToLower().StartsWith(text.ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string EncodeUrlBase64(this string base64String)
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                base64String = base64String.Replace('+', '-');
                base64String = base64String.Replace('/', '_');
                base64String = base64String.Replace('=', '!');
            }

            return base64String;
        }

        public static string DecodeUrlBase64(this string base64String)
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                base64String = base64String.Replace('-', '+');
                base64String = base64String.Replace('_', '/');
                base64String = base64String.Replace('!', '=');
            }

            return base64String;
        }

    }
}
