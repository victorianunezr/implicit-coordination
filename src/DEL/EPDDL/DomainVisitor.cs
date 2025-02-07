using System;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class DomainVisitor : EPDDLParserBaseVisitor<object>
    {
        private Domain domain;
        private FormulaVisitor formulaVisitor = new FormulaVisitor();
        private Action CurrentAction;
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
            List<Parameter> parameters = VisitTypedVariableList(context.typedVariableList());;

            Predicate predicate = new Predicate(predicateName, parameters);
            
            return predicate;
        }

        public override object VisitActionDef(EPDDLParser.ActionDefContext context)
        {
            var actionName = context.actionName().GetText();
            var action = new Action { name = actionName };
      
            action.Parameters = VisitParametersDef(context.parametersDef());

            accessibilityRelationVisitor.model = action;
            CurrentAction = action;

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
                action.AllowedAgents.Add(new Agent(agentName.AGENT_NAME().GetText()));
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

        public override List<Parameter> VisitParametersDef(EPDDLParser.ParametersDefContext context)
        {
            // context: PARAMETERS LPAREN typedVariableList RPAREN
            return VisitTypedVariableList(context.typedVariableList());
        }

        public override List<Parameter> VisitTypedVariableList(EPDDLParser.TypedVariableListContext context)
        {
            var parameters = new List<Parameter>();
            if (context.DASH() == null)
            {
                // First alternative: (VARIABLE)*
                foreach (var token in context.VARIABLE())
                {
                    parameters.Add(new Parameter(token.GetText(), "unknown"));
                }
            }
            else
            {
                // Second alternative: VARIABLE (VARIABLE)* DASH type typedVariableList
                // Collect head variables (those occurring before the DASH)
                foreach (var token in context.VARIABLE())
                {
                    if (token.Symbol.TokenIndex < context.DASH().Symbol.TokenIndex)
                        parameters.Add(new Parameter(token.GetText(), context.type().GetText()));
                }
                // Recurse on the tail if present
                var tail = context.typedVariableList();
                if (tail != null)
                    parameters.AddRange(VisitTypedVariableList(tail));
            }
            return parameters;
        }

    

        // Visit each event definition and initialize Event model
        public override IWorld VisitEventDef(EPDDLParser.EventDefContext context)
        {
            var eventName = context.NAME().GetText();
            var eventObj = new Event { Name = eventName };
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
                        Parameter param = CurrentAction.Parameters.FirstOrDefault(
                            p => p.Name.Equals(termText, StringComparison.OrdinalIgnoreCase));

                        if (param == null)
                        {
                            throw new KeyNotFoundException($"No parameter found with name '{termText}'.");
                        }
                        parameters.Add(param);
                    }

                    Predicate predicate = new Predicate(predicateName, parameters);

                    eventObj.effect.Add(predicate, !isNegated);
                }
            }
            return eventObj;
        }
    }
}