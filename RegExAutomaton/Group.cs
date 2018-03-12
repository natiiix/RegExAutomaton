namespace RegExAutomaton
{
    public class Group
    {
        public string Value { get; private set; }
        public bool Capture { get; private set; }
        public Quantifier Quantifier { get; private set; }

        public Group(string value, bool capture, Quantifier quantifier = Quantifier.None)
        {
            Value = value;
            Capture = capture;
            Quantifier = quantifier;
        }
    }
}
