using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImplicitCoordination.DEL
{
    public class PredicateRepository
    {
        public Dictionary<string, Predicate> Predicates = new Dictionary<string, Predicate>();
        
        public void Add(Predicate p)
        {
            Predicates.Add(p.name, p);
        }

        public Predicate Get(string name)
        {
            Predicate p;
            if (Predicates.TryGetValue(name, out p))
            {
                return p;
            }
            throw new Exception("Predicate not found");
        }
    }
}
