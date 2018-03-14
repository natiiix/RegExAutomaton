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

        private int LastStateIndex { get => states.Count - 1; }

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

            ProcessPattern(innerPattern);
        }

        private int AddState()
        {
            states.Add(new State());
            return LastStateIndex;
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

        //private void ProcessPattern(string pattern)
        //{
        //    // Example expression: ab|(ab*cd?efg|hello)+

        //    // Temporary variables

        //    int depth = 0;
        //    int captureGroupCount = 0;
        //    List<int> decisionStates = new List<int>();
        //    string sequence = string.Empty;

        //    // Initial setup

        //    states = new List<State>();
        //    edges = new List<Edge>();

        //    states.Add(new State());
        //    startingState = 0;

        //    decisionStates.Add(startingState);

        //    for (int i = 0, len = pattern.Length; i < len; i++)
        //    {
        //        string value = pattern[i].ToString();

        //        bool isMeta = pattern.ContainsAtUnescaped(i, Meta.All);
        //        Quantifier quant = pattern.GetQuantifier(i);

        //        if (isMeta || quant != Quantifier.None)
        //        {
        //            if (sequence.Length > 0)
        //            {
        //                int lastState = AddState();
        //                edges.Add(new Edge(lastState - 1, lastState, sequence));
        //                sequence = string.Empty;
        //            }

        //            if (isMeta)
        //            {
        //                // TODO

        //                if (pattern.ContainsAtUnescaped(i, Meta.GroupStart))
        //                {
        //                    depth++;
        //                    decisionStates.Add(states.Count - 1);

        //                    if (!pattern.ContainsAt(i + 1, Meta.NonCapture))
        //                    {
        //                        captureGroupCount++;
        //                    }
        //                }
        //                else if (pattern.ContainsAtUnescaped(i, Meta.GroupEnd))
        //                {
        //                    depth--;
        //                    decisionStates.RemoveLast();
        //                }
        //                else
        //                {
        //                    throw new NotImplementedException();
        //                }
        //            }
        //            else
        //            {
        //                switch (quant)
        //                {
        //                    case Quantifier.ZeroOrOne:
        //                        {
        //                            int lastState = AddState();
        //                            edges.Add(new Edge(lastState - 1, lastState, value));
        //                            edges.Add(new Edge(lastState - 1, lastState, string.Empty));
        //                        }
        //                        break;

        //                    case Quantifier.ZeroOrMore:
        //                        {
        //                            int lastState = states.Count - 1;
        //                            edges.Add(new Edge(lastState, lastState, value));
        //                        }
        //                        break;

        //                    case Quantifier.OneOrMore:
        //                        {
        //                            int lastState = AddState();
        //                            edges.Add(new Edge(lastState - 1, lastState, value));
        //                            edges.Add(new Edge(lastState, lastState, value));
        //                        }
        //                        break;

        //                    default:
        //                        throw new Exception();
        //                }
        //            }
        //        }
        //        else if (value != Meta.Escape || pattern.IsEscaped(i))
        //        {
        //            sequence += value;
        //        }
        //    }
        //}

        private void ProcessPattern(string pattern)
        {
            states = new List<State>();
            edges = new List<Edge>();

            startingState = AddState();

            List<int> decisionStates = new List<int>() { startingState };
            List<int> branchEndStates = new List<int>() { decisionStates.Last() };

            string sequence = string.Empty;

            for (int i = 0, len = pattern.Length, step = 1; i < len; i += step)
            {
                // Or
                if (pattern.ContainsAtUnescaped(i, Meta.Or))
                {
                    if (sequence != string.Empty)
                    {
                        edges.Add(new Edge(branchEndStates.Last(), AddState(), sequence));
                        branchEndStates[branchEndStates.Count - 1] = LastStateIndex;
                        branchEndStates.Add(decisionStates.Last());
                        sequence = string.Empty;
                    }

                    step = Meta.Or.Length;
                }
                // Zero or more quantifier
                else if (pattern.ContainsAtUnescaped(i + 1, Meta.ZeroOrMore))
                {
                    edges.Add(new Edge(branchEndStates.Last(), AddState(), sequence));
                    branchEndStates[branchEndStates.Count - 1] = LastStateIndex;

                    sequence = string.Empty;
                    step = 1 + Meta.ZeroOrMore.Length;

                    edges.Add(new Edge(LastStateIndex, LastStateIndex, pattern[i].ToString()));
                }
                // Unescaped escape character
                else if (pattern.ContainsAtUnescaped(i, Meta.Escape))
                {
                    step = 1;
                }
                // Literal or escaped character
                else
                {
                    sequence += pattern[i];
                    step = 1;
                }
            }

            if (sequence != string.Empty)
            {
                edges.Add(new Edge(branchEndStates.Last(), AddState(), sequence));
                branchEndStates[branchEndStates.Count - 1] = LastStateIndex;
                sequence = string.Empty;
            }

            foreach (int endState in branchEndStates)
            {
                states[endState].Ending = true;
            }
        }

        public Match Match(string str)
        {
            string fullCapture = string.Empty;

            for (int i = 0, len = (fixedStart ? 0 : str.Length); i <= len; i++)
            {
                if (IsMatch(str, i, startingState, ref fullCapture))
                {
                    return new Match(fullCapture, i);
                }
            }

            return null;
        }

        public bool IsMatch(string str) => Match(str) != null;

        private bool IsMatch(string str, int index, int state, ref string fullCapture)
        {
            IEnumerable<Edge> availableEdges = edges.Where(x => x.Origin == state);

            foreach (Edge edge in availableEdges)
            {
                if (str.ContainsAt(index, edge.Value) && IsMatch(str, index + edge.Value.Length, edge.Destination, ref fullCapture))
                {
                    fullCapture += edge.Value;
                    return true;
                }
            }

            return states[state].Ending && (!fixedEnd || index == str.Length);
        }
    }
}
