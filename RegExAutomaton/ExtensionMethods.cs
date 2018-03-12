using System;
using System.Collections.Generic;
using System.Linq;

namespace RegExAutomaton
{
    public static class ExtensionMethods
    {
        public static bool IndexInRange<T>(this IEnumerable<T> enumerable, int index) => index >= 0 && index < enumerable.Count();

        public static bool ContainsAt(this string str, int index, string value)
        {
            int strLen = str.Length;
            int valLen = value.Length;

            if (index < 0 || index + valLen > strLen)
            {
                return false;
            }

            for (int i = 0; i < valLen; i++)
            {
                if (str[index + i] != value[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ContainsAt(this string str, int index, params string[] values) => values.Any(x => str.ContainsAt(index, x));

        public static bool ContainsAtUnescaped(this string str, int index, string value) => !str.IsEscaped(index) && str.ContainsAt(index, value);

        public static bool ContainsAtUnescaped(this string str, int index, params string[] values) => !str.IsEscaped(index) && str.ContainsAt(index, values);

        public static string[] SplitOnIndices(this string str, IEnumerable<int> indices)
        {
            if (indices.Any(x => !str.IndexInRange(x)))
            {
                throw new ArgumentException();
            }

            string[] parts = new string[indices.Count() + 1];

            int partIdx = 0;
            int partStart = 0;

            foreach (int idx in indices)
            {
                int partLen = idx - partStart;
                parts[partIdx] = str.Substring(partStart, partLen);

                partIdx++;
                partStart += partLen + 1;
            }

            parts[partIdx] = str.Substring(partStart, str.Length - partStart);

            return parts;
        }

        public static string[] SplitOnUnescaped(this string str, string delimiter, bool zeroDepthOnly = false)
        {
            List<int> splitIndices = str.FindUnescaped(delimiter, zeroDepthOnly);
            return str.SplitOnIndices(splitIndices);
        }

        public static string SubstringBetween(this string str, int start, int end) => str.Substring(start, end - start + 1);

        public static bool IsEscaped(this string str, int index)
        {
            bool escaped = false;

            for (int i = index - 1; i >= 0; i--)
            {
                if (str.ContainsAt(i, Meta.Escape))
                {
                    escaped = !escaped;
                }
                else
                {
                    break;
                }
            }

            return escaped;
        }

        public static bool ContainsUnescaped(this string str, string value)
        {
            if (value == string.Empty)
            {
                return true;
            }

            for (int i = 0, strLen = str.Length - value.Length + 1; i < strLen; i++)
            {
                if (str.ContainsAt(i, value) && !str.IsEscaped(i))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsUnescaped(this string str, bool containsAll, params string[] values) =>
            containsAll ? values.Distinct().All(x => str.ContainsUnescaped(x)) : values.Distinct().Any(x => str.ContainsUnescaped(x));

        public static bool ContainsUnescaped(this string str, params string[] values) => str.ContainsUnescaped(false, values);

        public static List<int> FindUnescaped(this string str, string value, bool zeroDepthOnly = false)
        {
            List<int> indices = new List<int>();
            int depth = 0;

            for (int i = 0, strLen = str.Length - value.Length + 1; i < strLen; i++)
            {
                if (str.IsEscaped(i))
                {
                    continue;
                }

                if (str.ContainsAt(i, Meta.GroupStart))
                {
                    depth++;
                }
                else if (str.ContainsAt(i, Meta.GroupEnd))
                {
                    depth--;
                }
                else if ((!zeroDepthOnly || depth == 0) && str.ContainsAt(i, value))
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        public static Dictionary<string, List<int>> FindUnescaped(this string str, bool zeroDepthOnly, params string[] values)
        {
            Dictionary<string, List<int>> indicesPerValue = new Dictionary<string, List<int>>();

            foreach (string value in values.Distinct())
            {
                indicesPerValue[value] = str.FindUnescaped(value, zeroDepthOnly);
            }

            return indicesPerValue;
        }

        public static Dictionary<string, List<int>> FindUnescaped(this string str, params string[] values) => str.FindUnescaped(false, values);

        public static Quantifier GetQuantifier(this string str, int index)
        {
            if (str.ContainsAt(index, Meta.ZeroOrOne))
            {
                return Quantifier.ZeroOrOne;
            }
            else if (str.ContainsAt(index, Meta.ZeroOrMore))
            {
                return Quantifier.ZeroOrMore;
            }
            else if (str.ContainsAt(index, Meta.OneOrMore))
            {
                return Quantifier.OneOrMore;
            }
            else
            {
                return Quantifier.None;
            }
        }
    }
}
