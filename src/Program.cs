using System;
using static Functional.Formula;

namespace ImplicitCoodrination
{
    class Program
    {
        static void Main(string[] args)
        {
            Formula p = Formula.NewProposition(true);
            Formula q = Formula.NewProposition(true);
            Formula f = Formula.NewAnd(p, q);
            bool result = evaluate(f);
            Console.WriteLine(result);
        }
    }
}
