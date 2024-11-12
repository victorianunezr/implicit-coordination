using System;

namespace ImplicitCoordination.DEL
{
    public class EPDDLToDELVisitor : EPDDLParserBaseVisitor<object>
    {
        private Domain currentDomain;
        private Action currentAction;
        private Event currentEvent;
        private State initialState;
        private FormulaVisitor formulaVisitor = new FormulaVisitor();


        public override object VisitDomainDef(EPDDLParser.DomainDefContext context)
        {
            var domainName = context.domainName().GetText();
            Console.WriteLine($"Domain: {domainName}");
            return currentDomain;
        }

        public override object VisitActionDef(EPDDLParser.ActionDefContext context)
        {
            var actionName = context.actionName().GetText();
            var action = new Action { name = actionName };
            currentDomain.Actions.Add(action); // Use `action` here, not `currentAction`

            Console.WriteLine($"Action: {actionName}");

            // Visit each event definition within the action
            foreach (var eventContext in context.eventsDef().eventDef())
            {
                var eventInstance = VisitEventDef(eventContext) as Event;
                action.possibleWorlds.Add(eventInstance);
                action.designatedWorlds.Add(eventInstance);
            }

            return action;
        }

        // Visit parameters definition and populate the action parameters
        public override object VisitParametersDef(EPDDLParser.ParametersDefContext context)
        {
            foreach (var parameter in context.typedVariableList().VARIABLE())
            {
                var paramName = parameter.GetText();
                currentAction.Parameters.Add(new Parameter { Name = paramName });
                Console.WriteLine($"Parameter: {paramName}");
            }
            return null;
        }

        // Visit each event definition and initialize Event model
        public override IWorld VisitEventDef(EPDDLParser.EventDefContext context)
        {
            var eventName = context.NAME().GetText();
            var eventObj = new Event { name = eventName };
            Console.WriteLine($"Event: {eventName}");

            // Handle precondition if it exists
            var preconditionContext = context.formulaOrEmpty();
            if (preconditionContext != null)
            {
                eventObj.pre = formulaVisitor.Visit(preconditionContext);
            }

            // Handle effect as a list of literals
            var effectLiterals = new List<Literal>();
            var effectContext = context.literal();
            foreach (var literalContext in effectContext)
            {
                var literal = formulaVisitor.VisitLiteral(literalContext) as Literal;
                if (literal != null)
                {
                    effectLiterals.Add(literal);
                }
            }
            eventObj.Effect = effectLiterals;

            return eventObj;
        }

        // Visit precondition of an event
        public override object VisitPrecondition(EPDDLParser.PreconditionContext context)
        {
            var preconditionFormula = context.formulaOrEmpty().GetText();
            currentEvent.Precondition = preconditionFormula;
            Console.WriteLine($"Precondition: {preconditionFormula}");
            return null;
        }

        // Visit effect of an event and add literals to the event effects
        public override object VisitEffect(EPDDLParser.EffectContext context)
        {
            foreach (var literalContext in context.literal())
            {
                var literal = literalContext.GetText();
                currentEvent.Effects.Add(new Literal { Name = literal });
                Console.WriteLine($"Effect: {literal}");
            }
            return null;
        }

        // Implement other visit methods as needed for the remaining elements of EPDDL

        // Helper methods or utilities can also be added here
    }
}