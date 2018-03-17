using System;
using System.Collections.Generic;
using System.Linq;

namespace RegExAutomaton
{
    public class RegEx
    {
        private bool fixedStart;
        private bool fixedEnd;

        public List<State> States { get; private set; }
        public List<Edge> Edges { get; private set; }

        public int StartingState { get; private set; }

        private int LastStateIndex { get => States.Count - 1; }

        public RegEx(string pattern)
        {
            fixedStart = pattern.StartsWith(Meta.StringStart);
            fixedEnd = pattern.EndsWith(Meta.StringEnd);

            int startIdx = fixedStart ? Meta.StringStart.Length : 0;
            int innerLen = pattern.Length - startIdx - (fixedEnd ? Meta.StringEnd.Length : 0);

            string innerPattern = pattern.Substring(startIdx, innerLen);

            if (innerPattern.ContainsUnescaped(Meta.StringStart, Meta.StringEnd))
            {
                throw new ArgumentException();
            }

            ProcessGroups(innerPattern);
            ProcessPattern(innerPattern);
        }

        private int AddState()
        {
            States.Add(new State(States.Count));
            return LastStateIndex;
        }

        private void ProcessGroups(string pattern)
        {
            List<int> groupStarts = pattern.FindUnescaped(Meta.GroupStart);
            List<int> groupEnds = pattern.FindUnescaped(Meta.GroupEnd);

            if (groupStarts.Count != groupEnds.Count)
            {
                throw new ArgumentException();
            }

            for (int i = 0, len = groupEnds.Count; i < len; i++)
            {
                int endIdx = groupEnds[i];

                if (groupStarts.Where(x => x < endIdx).Count() < i + 1)
                {
                    throw new ArgumentException();
                }
            }
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
            States = new List<State>();
            Edges = new List<Edge>();

            StartingState = AddState();

            List<int> decisionStates = new List<int>() { StartingState };
            List<int> branchEndStates = new List<int>() { StartingState };
            List<int> branchesPerGroup = new List<int>() { 1 };

            string sequence = string.Empty;

            for (int i = 0, len = pattern.Length, step = 1; i < len; i += step)
            {
                // Or
                if (pattern.ContainsAtUnescaped(i, Meta.Or))
                {
                    PushSequenceIfNotEmpty(ref sequence, ref decisionStates, ref branchEndStates);

                    branchEndStates.Add(decisionStates.Last());
                    branchesPerGroup[branchesPerGroup.Count - 1]++;

                    step = Meta.Or.Length;
                }
                // Group start
                else if (pattern.ContainsAtUnescaped(i, Meta.GroupStart))
                {
                    PushSequenceIfNotEmpty(ref sequence, ref decisionStates, ref branchEndStates);

                    int endOfCurrentBranch = branchEndStates.Last();

                    if (decisionStates.Contains(endOfCurrentBranch) && pattern.ContainsAtUnescaped(pattern.FindEndOfGroup(i) + 1, Meta.ZeroOrMore))
                    {
                        int newState = AddState();
                        Edges.Add(new Edge(endOfCurrentBranch, newState, string.Empty));
                        endOfCurrentBranch = newState;
                    }

                    decisionStates.Add(endOfCurrentBranch);
                    branchEndStates.Add(endOfCurrentBranch);
                    branchesPerGroup.Add(1);

                    step = Meta.GroupStart.Length;
                }
                // Group end
                else if (pattern.ContainsAtUnescaped(i, Meta.GroupEnd))
                {
                    PushSequenceIfNotEmpty(ref sequence, ref decisionStates, ref branchEndStates);

                    if (pattern.ContainsAtUnescaped(i + 1, Meta.ZeroOrMore))
                    {
                        int lastDecisionState = decisionStates.Last();

                        for (int j = 0, count = branchesPerGroup.Last(); j < count; j++)
                        {
                            int branchEnd = branchEndStates.Pop();

                            if (Edges.Exists(x => x.Origin == branchEnd))
                            {
                                Edges.Add(new Edge(branchEnd, lastDecisionState, string.Empty));
                            }
                            else
                            {
                                foreach (Edge edge in Edges.Where(x => x.Destination == branchEnd))
                                {
                                    edge.ChangeDestination(lastDecisionState);
                                }

                                States.RemoveAt(branchEnd);

                                for (int k = 0, stateCount = States.Count; k < stateCount; k++)
                                {
                                    if (States[k].Id != k)
                                    {
                                        throw new Exception();
                                    }
                                }
                            }
                        }

                        branchEndStates[branchEndStates.Count - 1] = decisionStates.Last();
                        i += Meta.ZeroOrMore.Length;
                    }
                    else
                    {
                        // TODO: Optimize the non-quantified groups as well.
                        // Remove all the unnecessary states and edges.

                        int branchCount = branchesPerGroup.Last();

                        if (branchCount > 1)
                        {
                            int newState = AddState();

                            for (int j = 0, count = branchesPerGroup.Last(); j < count; j++)
                            {
                                int branchEnd = branchEndStates.Pop();
                                Edges.Add(new Edge(branchEnd, newState, string.Empty));
                            }

                            branchEndStates[branchEndStates.Count - 1] = newState;
                        }
                    }

                    branchesPerGroup.RemoveLast();
                    decisionStates.RemoveLast();

                    step = Meta.GroupEnd.Length;
                }
                // Single-character "zero or more" quantifier
                else if (pattern.ContainsAtUnescaped(i + 1, Meta.ZeroOrMore))
                {
                    PushSequenceIfNotEmpty(ref sequence, ref decisionStates, ref branchEndStates);

                    // Creating the "zero or more" quantifier loop on a decision state
                    // would result in "a*|zyx" matching "aaazyx"
                    if (decisionStates.Contains(LastStateIndex))
                    {
                        // It is necessary to add an epsilon edge leading to the "zero or more" quantifier loop
                        Edges.Add(new Edge(LastStateIndex, AddState(), string.Empty));
                        branchEndStates[branchEndStates.Count - 1] = LastStateIndex;
                    }

                    Edges.Add(new Edge(LastStateIndex, LastStateIndex, pattern[i].ToString()));
                    step = 1 + Meta.ZeroOrMore.Length;
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

            PushSequenceIfNotEmpty(ref sequence, ref decisionStates, ref branchEndStates);

            // Mark the end state of each branch as an ending state
            foreach (int endState in branchEndStates)
            {
                States[endState].Ending = true;
            }
        }

        private void PushSequenceIfNotEmpty(ref string sequence, ref List<int> decisionStates, ref List<int> branchEndStates)
        {
            if (sequence != string.Empty)
            {
                Edges.Add(new Edge(branchEndStates.Last(), AddState(), sequence));
                branchEndStates[branchEndStates.Count - 1] = LastStateIndex;
                sequence = string.Empty;
            }
        }

        public Match Match(string str)
        {
            string fullCapture = string.Empty;

            for (int i = 0, len = (fixedStart ? 0 : str.Length); i <= len; i++)
            {
                if (RecursiveMatch(str, i, StartingState, ref fullCapture))
                {
                    return new Match(fullCapture, i);
                }
            }

            return null;
        }

        public bool IsMatch(string str) => Match(str) != null;

        private bool RecursiveMatch(string str, int index, int state, ref string fullCapture)
        {
            IEnumerable<Edge> availableEdges = Edges.Where(x => x.Origin == state);

            foreach (Edge edge in availableEdges)
            {
                if (str.ContainsAt(index, edge.Value) && RecursiveMatch(str, index + edge.Value.Length, edge.Destination, ref fullCapture))
                {
                    fullCapture = edge.Value + fullCapture;
                    return true;
                }
            }

            return States[state].Ending && (!fixedEnd || index == str.Length);
        }

        public override string ToString() => $"States: {string.Join(", ", States.Select(x => x.Ending ? $"[{x.Id}]" : x.Id.ToString()))}{Environment.NewLine}Edges:{Environment.NewLine}{string.Join(Environment.NewLine, Edges.Select(x => $"{x.Origin} -> \"{x.Value}\" -> {x.Destination}"))}";
    }
}
