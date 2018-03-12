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

        public static string SubstringBefore(this string str, int end, bool includeEnd = false) => str.Substring(0, end + (includeEnd ? 1 : 0));

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
            int quantIdx = index + 1;

            if (str.ContainsAt(quantIdx, Meta.ZeroOrOne))
            {
                return Quantifier.ZeroOrOne;
            }
            else if (str.ContainsAt(quantIdx, Meta.ZeroOrMore))
            {
                return Quantifier.ZeroOrMore;
            }
            else if (str.ContainsAt(quantIdx, Meta.OneOrMore))
            {
                return Quantifier.OneOrMore;
            }
            else
            {
                return Quantifier.None;
            }
        }

        public static string GetString(this Quantifier quantifier)
        {
            switch (quantifier)
            {
                case Quantifier.ZeroOrOne:
                    return Meta.ZeroOrOne;

                case Quantifier.ZeroOrMore:
                    return Meta.ZeroOrMore;

                case Quantifier.OneOrMore:
                    return Meta.OneOrMore;

                case Quantifier.None:
                    return string.Empty;

                default:
                    throw new ArgumentException();
            }
        }

        public static List<Group> SplitIntoGroups(this string str)
        {
            List<int> groupStarts = str.FindUnescaped(Meta.GroupStart, true);
            List<int> groupEnds = str.FindUnescaped(Meta.GroupEnd, true);

            if (groupStarts.Count != groupEnds.Count)
            {
                throw new ArgumentException();
            }

            List<Group> groups = new List<Group>();

            int groupCount = groupStarts.Count;

            if (groupCount == 0)
            {
                return new List<Group>() { new Group(str, false) };
            }

            int nextGroupStart = 0;

            for (int i = 0; i < groupCount; i++)
            {
                int start = groupStarts[i];

                if (start > nextGroupStart)
                {
                    groups.Add(new Group(str.SubstringBetween(nextGroupStart, start - 1), false));
                }

                int end = groupEnds[i];
                int innerStart = start + 1;

                bool nonCapture = str.ContainsAt(start + 1, Meta.NonCapture);

                if (nonCapture)
                {
                    innerStart += Meta.NonCapture.Length;
                }

                Quantifier quantifier = str.GetQuantifier(end);

                string value = str.SubstringBetween(innerStart, end - 1);

                groups.Add(new Group(value, !nonCapture, quantifier));

                nextGroupStart = end + 1 + quantifier.GetString().Length;
            }

            if (nextGroupStart < str.Length)
            {
                groups.Add(new Group(str.Substring(nextGroupStart), true));
            }

            return groups;
        }
    }
}
