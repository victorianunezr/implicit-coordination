using System;

namespace ImplicitCoodrination.DEL
{
    public class Formula
    {
        private FormulaType type;
        private Formula child;
        private Formula leftChild;
        private Formula rightChild;
        private Proposition proposition;

        public bool Evaluate(State s, World w)
        {
            switch (type)
            {
                case FormulaType.Top:
                    return true;

                case FormulaType.Bottom:
                    return false;

                case FormulaType.Atom:
                    return w.valuation[proposition.id];

                case FormulaType.Not:
                    return !child.Evaluate(s, w);

                case FormulaType.And:
                    return leftChild.Evaluate(s, w) && rightChild.Evaluate(s, w);

                case FormulaType.Or:
                    return leftChild.Evaluate(s, w) || rightChild.Evaluate(s, w);

                case FormulaType.Knows:
                    return false;

                case FormulaType.CommonKnow:
                    return false;

                default:
                    return false;

            }
        }

        public Formula Top()
        {
            return new Formula { type = FormulaType.Top };
        }

        public Formula Bottom()
        {
            return new Formula { type = FormulaType.Bottom };
        }

        public Formula Not(Formula f)
        {
            return new Formula { type = FormulaType.Not, child = f };
        }

        public Formula Atom(Proposition p)
        {
            return new Formula { type = FormulaType.Atom, proposition = p };
        }

        public Formula And(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.And, leftChild = f1, rightChild = f2 };
        }

        public Formula Or(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.Or, leftChild = f1, rightChild = f2 };
        }

        public Formula Knows(Formula f)
        {
            return new Formula { type = FormulaType.Knows, child = f };
        }

        public Formula CommonKnow(Formula f)
        {
            return new Formula { type = FormulaType.CommonKnow, child = f };
        }

    }


    //public class Not : Formula
    //{
    //    public Not(Formula f)
    //    {
    //        this.type = FormulaType.And;
    //        this.child = f;
    //    }

    //    public override bool Evaluate(State s, World w)
    //    {
    //        return !child.Evaluate(State s, World w);
    //    }
    //}

    //public class And : Formula
    //{
    //    public And(Formula f1, Formula f2)
    //    {
    //        this.type = FormulaType.And;
    //        this.leftChild = f1;
    //        this.rightChild = f2;
    //    }

    //    public override bool Evaluate(State s, World w)
    //    {
    //        return leftChild.Evaluate(s, w) && rightChild.Evaluate(s, w);
    //    }
    //}

    //public class Or : Formula
    //{
    //    public Or(Formula f1, Formula f2)
    //    {
    //        this.type = FormulaType.Or;
    //        this.leftChild = f1;
    //        this.rightChild = f2;
    //    }

    //    public override bool Evaluate(State s, World w)
    //    {
    //        return leftChild.Evaluate(State s, World w) || rightChild.Evaluate(State s, World w);
    //    }
    //}

    //public class Atom : Formula
    //{
    //    public Atom(Proposition prop)
    //    {
    //        this.proposition = prop;
    //    }

    //    public override bool Evaluate()
    //    {
    //        return proposition;
    //    }
    //}
}
