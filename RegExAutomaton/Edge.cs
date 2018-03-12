namespace RegExAutomaton
{
    public class Edge
    {
        public int Origin { get; private set; }
        public int Destination { get; private set; }
        public string Value { get; private set; }

        public Edge(int origin, int destination, string value)
        {
            Origin = origin;
            Destination = destination;
            Value = value;
        }
    }
}
