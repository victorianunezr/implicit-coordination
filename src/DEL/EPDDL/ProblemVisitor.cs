using System;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class ProblemVisitor : EPDDLParserBaseVisitor<object>
    {
        private readonly FormulaVisitor formulaVisitor;
        private readonly Domain domain;
        private Problem problem;
        private AccessibilityRelationVisitor accessibilityRelationVisitor = new AccessibilityRelationVisitor();

        public ProblemVisitor(FormulaVisitor formulaVisitor, Domain domain)
        {
            this.formulaVisitor = formulaVisitor;
            this.domain = domain;
            this.problem = new Problem(domain);
        }

        public override object VisitProblemDef(EPDDLParser.ProblemDefContext context)
        {
            var problemName = context.problemName().GetText();
            this.problem.name = problemName;

            var initDefContextItem = context.problemItemDef().Where(item => item.initDef() != null);

            if (initDefContextItem.Any())
            {
                problem.initialState = VisitInitDef(initDefContextItem.First().initDef());
            }

            var goalDefContextItem = context.problemItemDef().Where(item => item.goalDef() != null);

            if (goalDefContextItem.Any())
            {
                problem.goalFormula = formulaVisitor.Visit(goalDefContextItem.First().goalDef().formula());
            }

            return problem;
        }

        public override State VisitInitDef(EPDDLParser.InitDefContext context)
        {
            var state = new State();

            this.accessibilityRelationVisitor.model = state;

            // Visit each world definition within the init
            var worlds = context.worldsDef().worldDef();
            foreach (var worldContext in worlds)
            {
                var worldInstance = VisitWorldDef(worldContext) as World;
                state.possibleWorlds.Add(worldInstance);
                state.designatedWorlds.Add(worldInstance);
            }

            if (context.accessibilityDef() != null)
            {
                var accessibilityRelation = accessibilityRelationVisitor.VisitAccessibilityDef(context.accessibilityDef());
                state.accessibility = accessibilityRelation;
            }

            problem.initialState = state;

            return state;
        }

        public override World VisitWorldDef(EPDDLParser.WorldDefContext context)
        {
            var worldName = context.worldName().GetText();
            var world = new World { Name = worldName };
            Console.WriteLine($"World: {worldName}");

            foreach (var predicateContext in context.predicate())
            {
                string predicateName = predicateContext.predicateName().GetText();

                List<Parameter> parameters = new();
                foreach (var termContext in predicateContext.term())
                {
                    string termText = termContext.GetText();
                    parameters.Add(new Parameter(termText));
                }
    
                Predicate predicate = new Predicate(predicateName, parameters);
                world.predicates.Add(predicate);
            }

            return world;
        }
    }
}