using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImplicitCoordination.DEL
{
    public static class PropositionRepository
    {
        public static Dictionary<string, Proposition> Propositions;
        
        public static void Add(Proposition p)
        {
            Propositions.Add(p.name, p);
        }

        public static Proposition Get(string name)
        {
            Proposition p;
            if (Propositions.TryGetValue(name, out p))
            {
                return p;
            }
            throw new Exception("Proposition not found");
        }
    }
}
