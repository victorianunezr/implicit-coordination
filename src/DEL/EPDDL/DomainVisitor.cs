using System;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class DomainVisitor : EPDDLParserBaseVisitor<object>
    {
        private Domain domain;
        private FormulaVisitor formulaVisitor = new FormulaVisitor();

        private Action currentAction;


        public override object VisitDomainDef(EPDDLParser.DomainDefContext context)
        {
            var domainName = context.domainName().GetText();

            this.domain = new Domain { name = domainName };
            Console.WriteLine($"Domain: {domainName}");
            foreach (var item in context.domainItemDef())
            {
                if (item.predicateListDef() != null)
                {
                    VisitPredicateListDef(item.predicateListDef());
                }
                else if (item.actionDef() != null)
                {
                    VisitActionDef(item.actionDef());
                }
                else
                {
                    Console.WriteLine("Unprocessed domain item detected.");
                }
            }
            return domain;
        }

        public override object VisitPredicateListDef(EPDDLParser.PredicateListDefContext context)
        {
            foreach (var predicateContext in context.predicateDef())
            {
                Predicate predicate = (Predicate)VisitPredicateDef(predicateContext);
                domain.AddPredicate(predicate);
            }
            
            return domain.Predicates;
        }

        public override object VisitPredicateDef(EPDDLParser.PredicateDefContext context)
        {
            string predicateName = context.predicateName().GetText();
            List<Parameter> parameters = new List<Parameter>();

            foreach (var variableContext in context.typedVariableList().VARIABLE())
            {
                string variableName = variableContext.GetText();
                // Check for type annotation after `-`, if present
                var typeContext = context.typedVariableList().type();
                string variableType = typeContext != null ? typeContext.GetText() : "unknown";

                parameters.Add(new Parameter(variableName, variableType));
            }

            Predicate predicate = new Predicate(predicateName, parameters);
            
            return predicate;
        }

        public override object VisitActionDef(EPDDLParser.ActionDefContext context)
        {
            var actionName = context.actionName().GetText();
            var action = new Action { name = actionName };
            currentAction = action;

            // Visit each event definition within the action
            var events = context.eventsDef().eventDef();
            foreach (var eventContext in events)
            {
                var eventInstance = VisitEventDef(eventContext) as Event;
                action.possibleWorlds.Add(eventInstance);
                action.designatedWorlds.Add(eventInstance);
            }

            foreach (var agentName in context.ownersDef().agentList().agentName())
            {
                action.owners.Add(new Agent(agentName.AGENT_NAME().GetText()));
            }

            if (context.accessibilityDef() != null)
            {
                var accessibilityRelation = VisitAccessibilityDef(context.accessibilityDef()) as AccessibilityRelation;
                action.accessibility = accessibilityRelation;
            }

            Console.WriteLine($"Action: {actionName}");
            domain.actions.Add(action);

            return action;
        }

        // Visit parameters definition and populate the action parameters
        // public override object VisitParametersDef(EPDDLParser.ParametersDefContext context)
        // {
        //     foreach (var parameter in context.typedVariableList().VARIABLE())
        //     {
        //         var paramName = parameter.GetText();
        //         currentAction.Parameters.Add(new Parameter { Name = paramName });
        //         Console.WriteLine($"Parameter: {paramName}");
        //     }
        //     return null;
        // }

        // Visit each event definition and initialize Event model
        public override IWorld VisitEventDef(EPDDLParser.EventDefContext context)
        {
            var eventName = context.NAME().GetText();
            var eventObj = new Event { name = eventName };
            Console.WriteLine($"Event: {eventName}");

            var preconditionContext = context.formulaOrEmpty();
            eventObj.pre = formulaVisitor.Visit(preconditionContext);

            var literals = context.effectDef().literal();
            if (literals != null)
            {
                // Handle effect as a set of literals
                foreach (var literalContext in literals)
                {
                    var predicateName = literalContext.predicate().predicateName().NAME().GetText();
                    bool isNegated = literalContext.NOT() != null;

                    List<Parameter> parameters = new();
                    foreach (var termContext in literalContext.predicate().term())
                    {
                        string termText = termContext.GetText();
                        parameters.Add(new Parameter(termText));
                    }

                    Predicate predicate = new Predicate(predicateName, parameters, isNegated);
                    eventObj.effect.Add(predicate);
                }
            }
            return eventObj;
        }

        public override object VisitAccessibilityDef(EPDDLParser.AccessibilityDefContext context)
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
                    var event1Name = relContext.NAME(0).GetText();
                    var event2Name = relContext.NAME(1).GetText();

                    // Retrieve the corresponding events
                    var event1 = currentAction.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == event1Name);
                    var event2 = currentAction.possibleWorlds.OfType<Event>().FirstOrDefault(e => e.name == event2Name);

                    if (event1 == null || event2 == null)
                    {
                        throw new InvalidOperationException($"Events {event1Name} or {event2Name} not found in action {currentAction.name}");
                    }

                    // Add the relation for each agent in the list
                    foreach (var agentContext in relContext.agentName())
                    {
                        var agentName = agentContext.GetText();

                        accessibilityRelation.AddEdge(new Agent(agentName), (event1, event2));
                    }
                }
            }

            return accessibilityRelation;
        }


    }
}