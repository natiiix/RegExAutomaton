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

        private int startingState;

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

            //states = new List<State>();
            //edges = new List<Edge>();

            //ProcessPattern(innerPattern);

            ProcessPatternAlt(innerPattern);
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

                if (groups != null)
                {
                    foreach (Group g in groups)
                    {
                        ProcessPattern(g.Value);
                    }
                }
                else
                {
                    // TODO: Process the group
                }
            }
        }

        private void ProcessPatternAlt(string pattern)
        {
            // Example expression: ab|(ab*cd?efg|hello)+

            // Temporary variables

            int depth = 0;
            int captureGroupCount = 0;
            List<int> decisionStates = new List<int>();
            string sequence = string.Empty;

            // Initial setup

            states = new List<State>();
            edges = new List<Edge>();

            states.Add(new State());
            startingState = 0;

            decisionStates.Add(startingState);

            for (int i = 0, len = pattern.Length; i < len; i++)
            {
                string value = pattern[i].ToString();

                bool isMeta = pattern.ContainsAtUnescaped(i, Meta.All);
                Quantifier quant = pattern.GetQuantifier(i);

                if (isMeta || quant != Quantifier.None)
                {
                    if (sequence.Length > 0)
                    {
                        int lastState = AddState();
                        edges.Add(new Edge(lastState - 1, lastState, sequence));
                        sequence = string.Empty;
                    }

                    if (isMeta)
                    {
                        // TODO

                        if (pattern.ContainsAtUnescaped(i, Meta.GroupStart))
                        {
                            depth++;
                            decisionStates.Add(states.Count - 1);

                            if (!pattern.ContainsAt(i + 1, Meta.NonCapture))
                            {
                                captureGroupCount++;
                            }
                        }
                        else if (pattern.ContainsAtUnescaped(i, Meta.GroupEnd))
                        {
                            depth--;
                            decisionStates.RemoveLast();
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        switch (quant)
                        {
                            case Quantifier.ZeroOrOne:
                                {
                                    int lastState = AddState();
                                    edges.Add(new Edge(lastState - 1, lastState, value));
                                    edges.Add(new Edge(lastState - 1, lastState, string.Empty));
                                }
                                break;

                            case Quantifier.ZeroOrMore:
                                {
                                    int lastState = states.Count - 1;
                                    edges.Add(new Edge(lastState, lastState, value));
                                }
                                break;

                            case Quantifier.OneOrMore:
                                {
                                    int lastState = AddState();
                                    edges.Add(new Edge(lastState - 1, lastState, value));
                                    edges.Add(new Edge(lastState, lastState, value));
                                }
                                break;

                            default:
                                throw new Exception();
                        }
                    }
                }
                else if (value != Meta.Escape || pattern.IsEscaped(i))
                {
                    sequence += value;
                }
            }
        }

        private int AddState()
        {
            states.Add(new State());
            return states.Count - 1;
        }
    }
}
