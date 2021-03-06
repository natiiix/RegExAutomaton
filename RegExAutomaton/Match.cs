﻿namespace RegExAutomaton
{
    public class Match
    {
        public int StartIndex { get; private set; }
        public string Value { get; private set; }
        public string[] Captures { get; private set; }

        public int Length { get => Value.Length; }
        public int EndIndex { get => StartIndex + Length - 1; }

        public Match(int startIndex, string value, string[] captures)
        {
            StartIndex = startIndex;
            Value = value;
            Captures = captures;
        }
    }
}
