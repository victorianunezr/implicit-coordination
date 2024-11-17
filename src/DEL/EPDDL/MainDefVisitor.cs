using System;
using ImplicitCoordination.DEL;
using Antlr4.Runtime;

public class MainDefVisitor : EPDDLParserBaseVisitor<object>
{
    public override object VisitMainDef(EPDDLParser.MainDefContext context)
    {
        if (context.domainDef() != null)
        {
            var domainVisitor = new DomainVisitor();
            return domainVisitor.Visit(context.domainDef());
        }
        else if (context.problemDef() != null)
        {
            throw new NotImplementedException("Problem definitions are not yet supported.");
        }
        else if (context.libraryDef() != null)
        {
            throw new NotImplementedException("Library definitions are not yet supported.");
        }

        throw new InvalidOperationException("Unrecognized main definition type.");
    }
}