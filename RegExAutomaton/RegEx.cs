using System;
using System.Collections.Generic;
using System.Linq;

namespace RegExAutomaton
{
    public class RegEx
    {
        private bool fixedStart;
        private bool fixedEnd;

        private List<State> states;
        private List<Edge> edges;

        public RegEx(string pattern)
        {
            if (!CheckGroupStartEndCount(pattern))
            {
                throw new ArgumentException();
            }

            fixedStart = pattern.StartsWith(Meta.StringStart);
            fixedEnd = pattern.EndsWith(Meta.StringEnd);

            int startIdx = fixedStart ? Meta.StringStart.Length : 0;
            int innerLen = pattern.Length - startIdx - (fixedEnd ? Meta.StringEnd.Length : 0);

            string innerPattern = pattern.Substring(startIdx, innerLen);

            if (innerPattern.ContainsUnescaped(Meta.StringStart, Meta.StringEnd))
            {
                throw new ArgumentException();
            }

            states = new List<State>();
            edges = new List<Edge>();

            ProcessPattern(innerPattern);
        }

        private static bool CheckGroupStartEndCount(string pattern)
        {
            List<int> groupStarts = pattern.FindUnescaped(Meta.GroupStart, true);
            List<int> groupEnds = pattern.FindUnescaped(Meta.GroupEnd, true);

            if (groupStarts.Count != groupEnds.Count)
            {
                return false;
            }

            for (int i = 0, len = groupEnds.Count; i < len; i++)
            {
                int endIdx = groupEnds[i];

                if (groupStarts.Where(x => x < endIdx).Count() < i + 1)
                {
                    return false;
                }
            }

            return true;
        }

        private void ProcessPattern(string pattern)
        {
            List<int> orIndices = pattern.FindUnescaped(Meta.Or, true);

            if (orIndices.Count > 0)
            {
                string[] alternatives = pattern.SplitOnIndices(orIndices);

                foreach (string alt in alternatives)
                {
                    // TODO
                }

                return;
            }
        }

        private static Group[] ExtractGroups(string pattern)
        {
            List<int> groupStarts = pattern.FindUnescaped(Meta.GroupStart, true);
            List<int> groupEnds = pattern.FindUnescaped(Meta.GroupEnd, true);

            if (groupStarts.Count != groupEnds.Count)
            {
                throw new ArgumentException();
            }

            int groupCount = groupStarts.Count;
            Group[] groups = new Group[groupCount];

            for (int i = 0; i < groupCount; i++)
            {
                int start = groupStarts[i];
                int end = groupEnds[i];

                Quantifier quantifier = pattern.GetQuantifier(end + 1);
                bool nonCapture = pattern.ContainsAt(start + 1, Meta.NonCapture);

                int innerStart = start + (nonCapture ? 3 : 1);
                int length = end - innerStart;

                string value = pattern.Substring(innerStart, length);

                groups[i] = new Group(value, !nonCapture, quantifier);
            }

            return groups;
        }
    }
}
