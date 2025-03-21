﻿namespace ImplicitCoordination.DEL
{
    public class Agent
    {
        private static ushort Counter = 0;
        private readonly ushort id;

        public ushort Id => this.id;

        public string name;

        public Agent(string name)
        {
            this.name = name;
            this.id = Counter;
            Counter++;
        }

        public static void ResetIdCounter()
        {
            Counter = 0;
        }

        public override bool Equals(object obj)
        {
            return obj is Agent other && name == other.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}