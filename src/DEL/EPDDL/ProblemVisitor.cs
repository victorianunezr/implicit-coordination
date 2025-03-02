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
            problem = new Problem();
            this.formulaVisitor = formulaVisitor;
            FormulaVisitor.Problem = problem;
            this.domain = domain;
        }

        public override object VisitProblemDef(EPDDLParser.ProblemDefContext context)
        {
            var problemName = context.problemName().GetText();
            problem.name = problemName;

            var objectDefContextItem = context.problemItemDef().Where(item => item.objectNamesDef() != null);
            if (objectDefContextItem.Any())
            {
                problem.Objects = VisitObjectNamesDef(objectDefContextItem.First().objectNamesDef());
            }

            var agents = ExtractAgents(problem.Objects);
            if (!agents.Any()) throw new Exception("Found no agents in Objects definition.");
            accessibilityRelationVisitor.agents = agents;

            PopulateAccessibilityOfActions(agents);

            problem.PopulateGroundPredicates(domain);

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

        public override IEnumerable<Object> VisitObjectNamesDef(EPDDLParser.ObjectNamesDefContext context)
        {
            var objects = new List<Object>();

            foreach (var lineCtx in context.typedLine())
            {
                objects.AddRange(VisitTypedLine(lineCtx));
            }

            return objects;
        }

        public HashSet<Agent> ExtractAgents(IEnumerable<Object> objects)
        {
            return objects
                .Where(o => o.Type.Equals("agent", StringComparison.OrdinalIgnoreCase))
                .Select(o => new Agent(o.Name))
                .ToHashSet();
        }

        // Add reflexive edges for all event models defined in domain, and agents defined in problem.
        public void PopulateAccessibilityOfActions(ICollection<Agent> agents)
        {
            foreach (Action action in domain.actions)
            {
                action.accessibility.AddAgents(agents, action.possibleWorlds);
            }
        }

        public override IEnumerable<Object> VisitTypedLine(EPDDLParser.TypedLineContext context)
        {
            var objects = new List<Object>();

            var nameTokens = context.NAME();
            var typeText = context.type().GetText();

            foreach (var nt in nameTokens)
            {
                string objName = nt.GetText();
                objects.Add(new Object(objName, typeText));
            }

            return objects;
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
            var world = new World(worldName, Problem.TotalGroundPredicatesCount);
            Console.WriteLine($"World: {worldName}");

            foreach (var predicateContext in context.predicate())
            {
                string predicateName = predicateContext.predicateName().GetText();

                List<Object> arguments = new List<Object>();
                foreach (var termContext in predicateContext.term())
                {
                    string termText = termContext.GetText();
                    Object obj = problem.GetObjectByName(termText);
                    if (obj == null) { throw new Exception($"Unknown object: {termText}"); }
                    arguments.Add(obj);
                }

                var gp = new GroundPredicate(predicateName, arguments);
                if (Problem.GroundPredicateToIndex.TryGetValue(gp, out int idx))
                {
                    world.Facts.Set(idx, true);
                }
                else
                {
                    Console.WriteLine($"Warning: {gp} not recognized in Problem.GroundPredicateToIndex!");
                }
            }

            return world;
        }
    }
}