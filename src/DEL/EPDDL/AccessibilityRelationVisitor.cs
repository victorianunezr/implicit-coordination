using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class AccessibilityRelationVisitor : EPDDLParserBaseVisitor<object>
    {
        public EpistemicModel model;
        public HashSet<Agent> agents = new();

        public override AccessibilityRelation VisitAccessibilityDef(EPDDLParser.AccessibilityDefContext context)
        {
            var accessibilityRelation = new AccessibilityRelation(agents);

            // Handle trivial accessibility definition
            if (context.TRIVIAL_DEF() != null)
            {
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
                    var world1 = model.possibleWorlds.FirstOrDefault(x => x.Name == world1Name);
                    var world2 = model.possibleWorlds.FirstOrDefault(x => x.Name == world2Name);

                    if (world1 == null || world2 == null)
                    {
                        throw new InvalidOperationException($"Events/worlds {world1Name} or {world2Name} not found in action/state");
                    }

                    var agentNames = relContext.NAME().Skip(2).Select(n => n.GetText()).ToList();

                    // Add the relation for each agent in the list
                    foreach (var agentName in agentNames)
                    {
                        try
                        {
                            accessibilityRelation.TryAddEdge(new Agent(agentName), (world1, world2));
                        }
                        catch (KeyNotFoundException)
                        {
                            throw new KeyNotFoundException("Agent found in accessibility definition does not match an agent define in Problem objects.");
                        }
                            
                    }
                }
            }

            return accessibilityRelation;
        }
    }
}