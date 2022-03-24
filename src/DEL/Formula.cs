using System;

namespace ImplicitCoordination.DEL
{
    public class Formula
    {
        private FormulaType type;
        private Formula child;
        private Formula leftChild;
        private Formula rightChild;
        private Proposition proposition;
        private Agent agent;

        /// <summary>
        /// Evaluates validity of a formula in a pointed world model (i.e. truth of a formula in a given world).
        /// </summary>
        /// <param name="s"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public bool Evaluate(State s, World w)
        {
            switch (type)
            {
                case FormulaType.Top:
                    return true;

                case FormulaType.Bottom:
                    return false;

                case FormulaType.Atom:
                    return w.valuation.GetValue(proposition.id);

                case FormulaType.Not:
                    return !child.Evaluate(s, w);

                case FormulaType.And:
                     return leftChild.Evaluate(s, w) && rightChild.Evaluate(s, w);

                case FormulaType.Or:
                    return leftChild.Evaluate(s, w) || rightChild.Evaluate(s, w);

                case FormulaType.Knows:

                    foreach (World v in s.accessibility.GetAccessibleWorlds(agent, w))
                    {
                        if (!child.Evaluate(s, v)) { return false; }
                    }
                    return true;

                case FormulaType.CommonKnow:
                    return false;

                default:
                    return false;

            }
        }

        /// <summary>
        /// Evaluates validity of formula in a model (state). Formula is valid if it is valid in every designated world
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Evaluate(State s)
        {
            foreach (World w in s.designatedWorlds)
            {
                if (!Evaluate(s, w)) return false;
            }

            return true;
        }

        public static Formula Top()
        {
            return new Formula { type = FormulaType.Top };
        }

        public static Formula Bottom()
        {
            return new Formula { type = FormulaType.Bottom };
        }

        public static Formula Not(Formula f)
        {
            return new Formula { type = FormulaType.Not, child = f };
        }

        public static Formula Atom(Proposition p)
        {
            return new Formula { type = FormulaType.Atom, proposition = p };
        }

        public static Formula And(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.And, leftChild = f1, rightChild = f2 };
        }

        public static Formula Or(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.Or, leftChild = f1, rightChild = f2 };
        }

        public static Formula Knows(Agent a, Formula f)
        {
            return new Formula { type = FormulaType.Knows, child = f, agent = a };
        }

        public static Formula CommonKnow(Formula f)
        {
            return new Formula { type = FormulaType.CommonKnow, child = f };
        }

    }
}
