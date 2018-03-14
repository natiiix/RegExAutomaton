namespace RegExAutomaton
{
    public class Match
    {
        public string Value { get; private set; }
        public int StartIndex { get; private set; }

        public int Length { get => Value.Length; }
        public int EndIndex { get => StartIndex + Length - 1; }

        public Match(string value, int startIndex)
        {
            Value = value;
            StartIndex = startIndex;
        }
    }
}
