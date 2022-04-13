namespace ImplicitCoordination.DEL
{
    public class Agent
    {
        private static ushort Counter = 0;
        private readonly ushort id;

        public ushort Id => this.id;

        public string name;

        public Agent(string name=null)
        {
            this.name = name;
            this.id = Counter;
            Counter++;
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }
    }
}