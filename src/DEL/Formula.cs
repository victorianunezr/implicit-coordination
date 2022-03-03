using System;
namespace implicit_coordination.DEL
{
    public abstract class Formula
    {
        public FormulaType type;
        public Formula child;
        public Formula leftChild;
        public Formula rightChild;
        public Proposition proposition;

        public abstract bool Evaluate();

    }

    public class Top : Formula
    {
        public Top()
        {
            this.type = FormulaType.Top;
        }

        public override bool Evaluate()
        {
            return true;
        }
    }

    public class Bottom : Formula
    {
        public Bottom()
        {
            this.type = FormulaType.Bottom;
        }

        public override bool Evaluate()
        {
            return true;
        }
    }

    public class Not : Formula
    {
        public Not(Formula f)
        {
            this.type = FormulaType.And;
            this.child = f;
        }

        public override bool Evaluate()
        {
            return !child.Evaluate();
        }
    }

    public class And : Formula
    {
        public And(Formula f1, Formula f2)
        {
            this.type = FormulaType.And;
            this.leftChild = f1;
            this.rightChild = f2;
        }

        public override bool Evaluate()
        {
            return leftChild.Evaluate() && rightChild.Evaluate();
        }
    }

    public class Or : Formula
    {
        public Or(Formula f1, Formula f2)
        {
            this.type = FormulaType.Or;
            this.leftChild = f1;
            this.rightChild = f2;
        }

        public override bool Evaluate()
        {
            return leftChild.Evaluate() || rightChild.Evaluate();
        }
    }

    public class Atom : Formula
    {
        public Atom(Proposition prop)
        {
            this.proposition = prop;
        }

        public override bool Evaluate()
        {
            return proposition.evaluation;
        }
    }
}
