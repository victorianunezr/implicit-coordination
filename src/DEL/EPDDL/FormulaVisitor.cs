using System;
using System.Collections.Generic;

namespace ImplicitCoordination.DEL
{
    public class FormulaVisitor : EPDDLParserBaseVisitor<Formula>
    {
        public Problem problem;

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
                if (formulas.Count == 2)
                {
                    return Formula.Or(formulas[0], formulas[1]);
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
                if (formulas.Count == 2)
                {
                    return Formula.And(formulas[0], formulas[1]);
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
                if (formulas.Count == 2)
                {
                    return Formula.Or(formulas[0], formulas[1]);
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
                if (formulas.Count == 2)
                {
                    return Formula.And(formulas[0], formulas[1]);
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

            var paramArgs = new List<Parameter>();
            var objectArgs = new List<Object>();

            bool hasVariable = false;

            foreach (var termCtx in context.term())
            {
                // If it's a variable, mark hasVariable = true, and store as a Parameter
                if (termCtx.VARIABLE() != null)
                {
                    hasVariable = true;
                    string varName = termCtx.GetText(); 
                    paramArgs.Add(new Parameter(varName, "variable"));
                }
                else if (termCtx.groundTerm() != null)
                {
                    // It's a ground term with a NAME token, e.g. "pos0"
                    string nameText = termCtx.groundTerm().GetText();
                    Object obj = problem.GetObjectByName(nameText);
                    if (obj == null) { throw new Exception($"Unknown object: {nameText}"); }
                    objectArgs.Add(obj);
                }
                else
                {
                }
            }

            if (hasVariable)
            {
                // If any term is a variable, treat the whole predicate as schematic:
                var schematicPred = new Predicate(predicateName, paramArgs);
                return Formula.Atom(schematicPred);
            }
            else
            {
                // If no variables, it's fully grounded:
                var groundPred = new GroundPredicate(predicateName, objectArgs);
                return Formula.Atom(groundPred);
            }
        }
    }
}
