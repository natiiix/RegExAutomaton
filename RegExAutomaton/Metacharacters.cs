namespace RegExAutomaton
{
    public static class Meta
    {
        // Escape and non-capture symbols are not included
        public static string[] All { get => new string[] { StringStart, StringEnd, GroupStart, GroupEnd, Or, ZeroOrOne, ZeroOrMore, OneOrMore }; }

        public const string Escape = "\\";

        public const string StringStart = "^";
        public const string StringEnd = "$";

        public const string GroupStart = "(";
        public const string GroupEnd = ")";
        public const string NonCapture = "?:";

        public const string Or = "|";

        public const string ZeroOrOne = "?";
        public const string ZeroOrMore = "*";
        public const string OneOrMore = "+";
    }
}
