using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class FormulaVisitor : EPDDLParserBaseVisitor<Formula>
    {
        public override Formula VisitFormulaOrEmpty(EPDDLParser.FormulaOrEmptyContext context)
        {
            if (context == null || context.formula() == null)
            {
                return Formula.Top();
            }
            return Visit(context.formula());
        }
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
                return VisitPredicate(context.predicate());
            }
        }

        public override Formula VisitPredicate(EPDDLParser.PredicateContext context)
        {
            string predicateName = context.predicateName().GetText();
            List<Parameter> parameters = new List<Parameter>();

            foreach (var termContext in context.term())
            {
                string termText = termContext.GetText();
                string parameterType = null;

                if (termContext.VARIABLE() != null)
                {
                    parameterType = "variable";
                }
                else if (termContext.groundTerm() != null)
                {
                    parameterType = termContext.groundTerm().NAME() != null ? "ground" : "agent";
                }

                parameters.Add(new Parameter(termText, parameterType));
            }

            return Formula.Atom(new Predicate(predicateName, parameters));
        }
    }
}
