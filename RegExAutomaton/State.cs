namespace RegExAutomaton
{
    public class State
    {
        public bool Ending { get; private set; }

        public State(bool ending = false)
        {
            Ending = ending;
        }
    }
}
