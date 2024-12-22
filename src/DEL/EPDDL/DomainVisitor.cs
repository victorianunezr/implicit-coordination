using System;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class DomainVisitor : EPDDLParserBaseVisitor<object>
    {
        private Domain domain;
        private FormulaVisitor formulaVisitor = new FormulaVisitor();

        private AccessibilityRelationVisitor accessibilityRelationVisitor = new AccessibilityRelationVisitor();


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
            this.accessibilityRelationVisitor.model = action;

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
                var accessibilityRelation = accessibilityRelationVisitor.VisitAccessibilityDef(context.accessibilityDef());
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

                    Predicate predicate = new Predicate(predicateName, parameters);

                    eventObj.effect.Add(predicate, !isNegated);
                }
            }
            return eventObj;
        }
    }
}