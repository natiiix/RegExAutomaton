using System.Collections.Generic;

namespace RegExAutomaton
{
    public class Edge
    {
        public int Origin { get; private set; }
        public int Destination { get; private set; }
        public string Value { get; private set; }
        public int[] CaptureGroups { get; private set; }

        public Edge(int origin, int destination, string value, IEnumerable<int> captureGroups)
        {
            Origin = origin;
            Destination = destination;
            Value = value;
            CaptureGroups = captureGroups.CopyToArray();
        }

        public void ChangeDestination(int destination)
        {
            Destination = destination;
        }
    }
}
