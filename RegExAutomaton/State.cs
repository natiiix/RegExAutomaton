namespace RegExAutomaton
{
    public class State
    {
        public int Id { get; private set; }
        public bool Ending { get; set; } = false;

        public State(int id)
        {
            Id = id;
        }
    }
}
