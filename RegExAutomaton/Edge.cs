namespace RegExAutomaton
{
    public class Edge
    {
        public int Origin { get; private set; }
        public int Destination { get; private set; }
        public string Value { get; private set; }
        public int CaptureGroup { get; private set; }

        public Edge(int origin, int destination, string value, int captureGroup = -1)
        {
            Origin = origin;
            Destination = destination;
            Value = value;
            CaptureGroup = captureGroup;
        }
    }
}
