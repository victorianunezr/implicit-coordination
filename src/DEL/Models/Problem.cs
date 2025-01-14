using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class Problem
    {
        public string name { get; set; }
        public State initialState { get; set; }
        public Formula goalFormula { get; set; }

        public Problem(Domain domain)
        {
        }
    }
}