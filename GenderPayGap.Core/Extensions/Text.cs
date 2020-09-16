using System;
using System.Collections.Generic;
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

        public static bool IsCompanyNumber(this string input, int minLength = 8)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            if (!input.ContainsNumber())
            {
                return false;
            }

            return Regex.IsMatch(input, "^[0-9A-Za-z]{" + minLength + ",8}$");
        }

        public static string FixCompanyNumber(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.IsCompanyNumber(6))
            {
                input = input.PadLeft(8, '0');
            }

            return input;
        }

        public static bool ContainsNumber(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return Regex.IsMatch(input, "[0-9]");
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

        /// <summary>
        ///     Returns the name of the variable between $( and )
        /// </summary>
        /// <param name="text">The text to search</param>
        /// <param name="matchPattern">The pattern to use</param>
        /// <returns></returns>
        public static string GetVariableName(this string text, string matchPattern = @"^\$\((.*)\)$")
        {
            return new Regex(matchPattern).Matches(text)?.FirstOrDefault()?.Groups[1]?.Value;
        }

        public static string TrimI(this string source, string trimChars)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(trimChars))
            {
                return source;
            }

            return source.Trim(trimChars.ToCharArray());
        }

        public static string TrimI(this string source, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return trimChars == null || trimChars.Length == 0 ? source.Trim() : source.Trim(trimChars);
        }

        public static string TrimSuffix(this string source, string suffix)
        {
            if (source.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
            {
                source = source.Remove(source.Length - suffix.Length);
            }

            return source;
        }

        public static string Strip(this string text, string excludeChars)
        {
            if (text == null)
            {
                return null;
            }

            string newText = null;
            for (var i = 0; i < text.Length; i++)
            {
                if (excludeChars.IndexOf(text[i]) < 0)
                {
                    newText += text[i];
                }
            }

            return newText;
        }

        public static string Coalesce(this string text, params string[] options)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            foreach (string option in options)
            {
                if (!string.IsNullOrWhiteSpace(option))
                {
                    return option;
                }
            }

            return null;
        }

        //Checks if a byte sequence starts with another byte sequence

        public static int LineCount(this string text, string newLineChars = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(newLineChars))
            {
                newLineChars = Environment.NewLine;
            }

            text = text.Replace(newLineChars, "\n");
            string[] args = text.SplitI("\n");
            return args.Length;
        }

        /// <summary>
        ///     Returns all characters before the first occurrence of a string
        /// </summary>
        public static string BeforeFirst(this string text,
            string separator,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool inclusive = false,
            bool includeWhenNoSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            int i = text.IndexOf(separator, 0, comparisionType);
            if (i > -1)
            {
                return text.Substring(0, inclusive ? i + 1 : i);
            }

            return includeWhenNoSeparator ? text : null;
        }

        /// <summary>
        ///     Returns all characters after the first occurrence of a string
        /// </summary>
        public static string AfterFirst(this string text,
            string separator,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool includeSeparator = false,
            bool includeWhenNoSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            int i = text.IndexOf(separator, 0, comparisionType);
            if (i > -1)
            {
                return text.Substring(includeSeparator ? i : i + separator.Length);
            }

            return includeWhenNoSeparator ? text : null;
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

        public static bool EndsWithI(this string source, params string[] strings)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            foreach (string str in strings)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    continue;
                }

                if (source.ToLower().EndsWith(str.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartsWithAny(this string source, params char[] chars)
        {
            return source.Length > 0 && source[0].IsAny(chars);
        }

        /// <summary>
        ///     Super-fast Case-Insensitive text replace
        /// </summary>
        /// <param name="text">The original text string</param>
        /// <param name="fromStr">The string to search for</param>
        /// <param name="toStr">The string to replace with</param>
        /// <returns></returns>
        public static string ReplaceI(this string original, string pattern, string replacement = null)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(replacement))
            {
                replacement = "";
            }

            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
            var chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(
                       upperPattern,
                       position0))
                   != -1)
            {
                for (int i = position0; i < position1; ++i)
                {
                    chars[count++] = original[i];
                }

                for (var i = 0; i < replacement.Length; ++i)
                {
                    chars[count++] = replacement[i];
                }

                position0 = position1 + pattern.Length;
            }

            if (position0 == 0)
            {
                return original;
            }

            for (int i = position0; i < original.Length; ++i)
            {
                chars[count++] = original[i];
            }

            return new string(chars, 0, count);
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

        public static bool LikeAny(this string input, IEnumerable<string> patterns)
        {
            foreach (string pattern in patterns)
            {
                if (input.Like(pattern))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Like(this string input, string pattern)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            input = input.ToLower();
            pattern = pattern.ToLower().Trim();
            if (input == pattern)
            {
                return true;
            }

            string expression = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".").Replace("+", "\\+") + "$";
            return Regex.IsMatch(input, expression);
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

        public static int LevenshteinCompute(this string s, string t, bool caseIndensitive = true)
        {
            if (caseIndensitive)
            {
                s = s.ToLower();
                t = t.ToLower();
            }

            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++) { }

            for (var j = 0; j <= m; d[0, j] = j++) { }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                //Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = t[j - 1] == s[i - 1] ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

        public static string ToAbbr(this string s, string separator = "", int minLength = 3, params string[] excludeWords)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            List<string> wordList = s.ToLower().SplitI(" .-;:_,&+[]{}<>()").ToList();

            if (excludeWords != null && excludeWords.Length > 0)
            {
                wordList = wordList.Except(excludeWords, StringComparer.OrdinalIgnoreCase).ToList();
            }

            if (wordList.Count < minLength)
            {
                return null;
            }

            return string.Join(separator, wordList.Select(x => x[0]));
        }

    }
}
