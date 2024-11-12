using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class FormulaVisitor : EPDDLParserBaseVisitor<Formula>
    {
        public override Formula VisitFormula(EPDDLParser.FormulaContext context)
        {
            if (context.IMPLY() != null)
            {
                var left = Visit(context.formula(0));
                var right = Visit(context.formula(1));
                return Formula.Implies(left, right);
            }
            else if (context.OR() != null)
            {
                var formulas = new List<Formula>();
                foreach (var formulaContext in context.formula())
                {
                    formulas.Add(Visit(formulaContext));
                }
                return Formula.Disjunction(formulas);
            }
            else if (context.AND() != null)
            {
                var formulas = new List<Formula>();
                foreach (var formulaContext in context.formula())
                {
                    formulas.Add(Visit(formulaContext));
                }
                return Formula.Conjunction(formulas);
            }
            else if (context.NOT() != null)
            {
                var child = Visit(context.formula(0));
                return Formula.Not(child);
            }
            else if (context.TRUE() != null)
            {
                return Formula.Top();
            }
            else if (context.FALSE() != null)
            {
                return Formula.Bottom();
            }
            else
            {
                return Visit(context.predicateFormula());
            }
        }

        public override Formula VisitPredicateFormula(EPDDLParser.PredicateFormulaContext context)
        {
            if (context.IMPLY() != null)
            {
                var left = VisitPredicateFormula(context.predicateFormula(0));
                var right = VisitPredicateFormula(context.predicateFormula(1));
                return Formula.Implies(left, right);
            }
            else if (context.OR() != null)
            {
                var formulas = new List<Formula>();
                foreach (var subContext in context.predicateFormula())
                {
                    formulas.Add(VisitPredicateFormula(subContext));
                }
                return Formula.Disjunction(formulas);
            }
            else if (context.AND() != null)
            {
                var formulas = new List<Formula>();
                foreach (var subContext in context.predicateFormula())
                {
                    formulas.Add(VisitPredicateFormula(subContext));
                }
                return Formula.Conjunction(formulas);
            }
            else if (context.NOT() != null)
            {
                var child = VisitPredicateFormula(context.predicateFormula(0));
                return Formula.Not(child);
            }
            else
            {
                var propositionName = context.predicateName().GetText();
                var proposition = new Proposition(propositionName);
                return Formula.Atom(proposition);
            }
        }
    }
}