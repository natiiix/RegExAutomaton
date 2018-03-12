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
            // TODO:
            // - Create an interface for both alternatives and groups.
            // - Split the pattern into elementary groups and alternatives.
            // - Process the elementary blocks one by one.

            string[] alternatives = pattern.SplitOnUnescaped(Meta.Or, true);

            foreach (string alt in alternatives)
            {
                List<Group> groups = alt.SplitIntoGroups();

                if (groups.Count > 1)
                {
                    foreach (Group g in groups)
                    {
                    }
                }
                else
                {
                    // TODO: Process the group
                }
            }
        }
    }
}
