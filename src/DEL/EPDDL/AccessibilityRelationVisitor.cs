using System;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class AccessibilityRelationVisitor : EPDDLParserBaseVisitor<object>
    {
        public EpistemicModel model;

        public override AccessibilityRelation VisitAccessibilityDef(EPDDLParser.AccessibilityDefContext context)
        {
            var accessibilityRelation = new AccessibilityRelation();

            // Handle trivial accessibility definition
            if (context.TRIVIAL_DEF() != null)
            {
                Console.WriteLine("Trivial accessibility relation detected. No edges to add.");
                return accessibilityRelation;
            }

            // Handle non-trivial accessibility definition
            if (context.accessibilityRel() != null)
            {
                foreach (var relContext in context.accessibilityRel())
                {
                    var world1Name = relContext.NAME(0).GetText();
                    var world2Name = relContext.NAME(1).GetText();

                    // Retrieve the corresponding events
                    var world1 = model.possibleWorlds.FirstOrDefault(x => x.name == world1Name);
                    var world2 = model.possibleWorlds.FirstOrDefault(x => x.name == world2Name);

                    if (world1 == null || world2 == null)
                    {
                        throw new InvalidOperationException($"Events/worlds {world1Name} or {world2Name} not found in action/state");
                    }

                    // Add the relation for each agent in the list
                    foreach (var agentContext in relContext.agentName())
                    {
                        var agentName = agentContext.GetText();

                        accessibilityRelation.AddEdge(new Agent(agentName), (world1, world2));
                    }
                }
            }

            return accessibilityRelation;
        }
    }
}