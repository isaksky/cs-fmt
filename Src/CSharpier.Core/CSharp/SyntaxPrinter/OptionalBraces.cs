using CSharpier.Core.CSharp.SyntaxPrinter.SyntaxNodePrinters;
using CSharpier.Core.DocTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core.CSharp.SyntaxPrinter;

internal static class OptionalBraces
{
    public static Doc Print(StatementSyntax node, PrintingContext context)
    {
        if (node is BlockSyntax blockSyntax)
        {
            return Block.Print(blockSyntax, context);
        }

        if (context.Options.PreferBraces)
        {
            // Wrap single statement in braces
            var braceLeading = context.Options.BraceNewLine ? (Doc)Doc.Line : (Doc)" ";
            return Doc.Group(
                braceLeading,
                "{",
                Doc.Indent(Doc.HardLine, Node.Print(node, context)),
                Doc.HardLine,
                "}"
            );
        }

        return DocUtilities.RemoveInitialDoubleHardLine(
            Doc.Indent(Doc.HardLine, Node.Print(node, context))
        );
    }
}
